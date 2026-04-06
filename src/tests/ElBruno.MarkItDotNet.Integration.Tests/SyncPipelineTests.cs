// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text;
using ElBruno.MarkItDotNet.Chunking;
using ElBruno.MarkItDotNet.CoreModel;
using ElBruno.MarkItDotNet.Sync;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Integration.Tests;

/// <summary>
/// Tests the sync pipeline: chunk → hash → plan → commit → re-sync.
/// </summary>
public class SyncPipelineTests
{
    [Fact]
    public async Task FirstSync_PlansAddForAllChunks()
    {
        var document = TestDocumentFactory.CreateRealisticReport();
        var chunker = new HeadingBasedChunker();
        var chunks = chunker.Chunk(document);

        var store = new InMemorySyncStateStore();
        var executor = new SyncExecutor(store);
        var sourceStream = CreateSourceStream(document);

        var plan = await executor.PlanAsync(document.Id, sourceStream, chunks);

        plan.Action.Should().Be(SyncAction.Add);
        plan.ChunksToAdd.Should().HaveCount(chunks.Count);
        plan.ChunksToUpdate.Should().BeEmpty();
        plan.ChunksToDelete.Should().BeEmpty();
        plan.ChunksUnchanged.Should().BeEmpty();
        plan.PreviousVersion.Should().BeNull();
        plan.NewVersion.Should().Be(1);
    }

    [Fact]
    public async Task ResyncWithSameContent_PlansSkip()
    {
        var document = TestDocumentFactory.CreateRealisticReport();
        var chunker = new HeadingBasedChunker();
        var chunks = chunker.Chunk(document);

        var store = new InMemorySyncStateStore();
        var executor = new SyncExecutor(store);

        // First sync
        var sourceBytes = GetSourceBytes(document);
        var plan1 = await executor.PlanAsync(document.Id, new MemoryStream(sourceBytes), chunks);

        var chunkHashes = ContentHasher.ComputeChunkHashes(chunks);

        // Commit the first sync — manually save state with correct source hash
        var sourceHash = ContentHasher.ComputeSourceHash(sourceBytes);
        await store.SaveStateAsync(new SyncState
        {
            DocumentId = document.Id,
            SourceHash = sourceHash,
            ChunkHashes = chunkHashes,
            Version = plan1.NewVersion,
            LastSyncedAt = DateTimeOffset.UtcNow,
        });

        // Re-sync with same content
        var plan2 = await executor.PlanAsync(document.Id, new MemoryStream(sourceBytes), chunks);

        plan2.Action.Should().Be(SyncAction.Skip);
        plan2.ChunksToAdd.Should().BeEmpty();
        plan2.ChunksToUpdate.Should().BeEmpty();
        plan2.ChunksToDelete.Should().BeEmpty();
        plan2.ChunksUnchanged.Should().HaveCount(chunks.Count);
    }

    [Fact]
    public async Task ModifiedContent_PlansUpdateWithDiffs()
    {
        var document = TestDocumentFactory.CreateRealisticReport();
        var chunker = new HeadingBasedChunker();
        var chunks = chunker.Chunk(document);

        var store = new InMemorySyncStateStore();
        var sourceBytes = GetSourceBytes(document);
        var sourceHash = ContentHasher.ComputeSourceHash(sourceBytes);
        var chunkHashes = ContentHasher.ComputeChunkHashes(chunks);

        // Save initial state
        await store.SaveStateAsync(new SyncState
        {
            DocumentId = document.Id,
            SourceHash = sourceHash,
            ChunkHashes = chunkHashes,
            Version = 1,
            LastSyncedAt = DateTimeOffset.UtcNow,
        });

        // Modify the document: change one section's content
        var modifiedDocument = document with
        {
            Sections = document.Sections.Select((section, idx) =>
            {
                if (idx == 0)
                {
                    return section with
                    {
                        Blocks =
                        [
                            new ParagraphBlock
                            {
                                Id = DocumentIdGenerator.ForBlock("paragraph", "Updated executive summary content.", 100),
                                Text = "Updated executive summary content with entirely new information about the reorganization and strategic pivot that occurred in Q4 of the fiscal year.",
                                Source = new SourceReference { FilePath = "reports/annual-report.pdf", PageNumber = 1 },
                            },
                        ],
                    };
                }
                return section;
            }).ToList(),
        };

        var modifiedChunks = chunker.Chunk(modifiedDocument);
        var modifiedSourceBytes = GetSourceBytes(modifiedDocument);

        var executor = new SyncExecutor(store);
        var plan = await executor.PlanAsync(document.Id, new MemoryStream(modifiedSourceBytes), modifiedChunks);

        plan.Action.Should().Be(SyncAction.Update);

        // There should be some combination of adds, updates, or deletes
        var totalChanges = plan.ChunksToAdd.Count + plan.ChunksToUpdate.Count + plan.ChunksToDelete.Count;
        totalChanges.Should().BeGreaterThan(0, "modifying content should produce changes in the sync plan");
    }

    [Fact]
    public async Task SyncState_PersistsInStore()
    {
        var document = TestDocumentFactory.CreateRealisticReport();
        var chunker = new HeadingBasedChunker();
        var chunks = chunker.Chunk(document);

        var store = new InMemorySyncStateStore();
        var sourceBytes = GetSourceBytes(document);
        var sourceHash = ContentHasher.ComputeSourceHash(sourceBytes);
        var chunkHashes = ContentHasher.ComputeChunkHashes(chunks);

        var state = new SyncState
        {
            DocumentId = document.Id,
            SourceHash = sourceHash,
            ChunkHashes = chunkHashes,
            Version = 1,
            LastSyncedAt = DateTimeOffset.UtcNow,
        };

        await store.SaveStateAsync(state);

        var retrieved = await store.GetStateAsync(document.Id);
        retrieved.Should().NotBeNull();
        retrieved!.DocumentId.Should().Be(document.Id);
        retrieved.SourceHash.Should().Be(sourceHash);
        retrieved.ChunkHashes.Should().HaveCount(chunks.Count);
        retrieved.Version.Should().Be(1);
    }

    [Fact]
    public async Task MarkDeleted_ProducesDeletePlan()
    {
        var document = TestDocumentFactory.CreateRealisticReport();
        var chunker = new HeadingBasedChunker();
        var chunks = chunker.Chunk(document);

        var store = new InMemorySyncStateStore();
        var sourceBytes = GetSourceBytes(document);
        var sourceHash = ContentHasher.ComputeSourceHash(sourceBytes);
        var chunkHashes = ContentHasher.ComputeChunkHashes(chunks);

        await store.SaveStateAsync(new SyncState
        {
            DocumentId = document.Id,
            SourceHash = sourceHash,
            ChunkHashes = chunkHashes,
            Version = 1,
            LastSyncedAt = DateTimeOffset.UtcNow,
        });

        var executor = new SyncExecutor(store);
        var deletePlan = await executor.MarkDeletedAsync(document.Id);

        deletePlan.Action.Should().Be(SyncAction.Delete);
        deletePlan.ChunksToDelete.Should().HaveCount(chunks.Count);
        deletePlan.ChunksToAdd.Should().BeEmpty();
        deletePlan.ChunksToUpdate.Should().BeEmpty();

        // Verify state is marked as deleted
        var state = await store.GetStateAsync(document.Id);
        state.Should().NotBeNull();
        state!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void ContentHasher_ProducesDeterministicHashes()
    {
        var document = TestDocumentFactory.CreateRealisticReport();
        var chunker = new HeadingBasedChunker();
        var chunks = chunker.Chunk(document);

        var hashes1 = ContentHasher.ComputeChunkHashes(chunks);
        var hashes2 = ContentHasher.ComputeChunkHashes(chunks);

        hashes1.Should().BeEquivalentTo(hashes2, "hashing the same chunks should produce identical results");
    }

    private static MemoryStream CreateSourceStream(Document document)
    {
        var json = DocumentSerializer.Serialize(document);
        return new MemoryStream(Encoding.UTF8.GetBytes(json));
    }

    private static byte[] GetSourceBytes(Document document)
    {
        var json = DocumentSerializer.Serialize(document);
        return Encoding.UTF8.GetBytes(json);
    }
}
