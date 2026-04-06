// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text;
using ElBruno.MarkItDotNet.Chunking;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Sync.Tests;

public class SyncExecutorTests
{
    private readonly InMemorySyncStateStore _store = new();
    private readonly SyncExecutor _executor;

    public SyncExecutorTests()
    {
        _executor = new SyncExecutor(_store);
    }

    [Fact]
    public async Task PlanAsync_NewDocument_ReturnsAddPlan()
    {
        var content = Encoding.UTF8.GetBytes("Hello world");
        using var stream = new MemoryStream(content);
        var chunks = new List<ChunkResult>
        {
            new() { Id = "c1", Content = "Hello", HeadingPath = "Intro" },
            new() { Id = "c2", Content = "world", HeadingPath = "Intro" }
        };

        var plan = await _executor.PlanAsync("doc-1", stream, chunks);

        plan.Action.Should().Be(SyncAction.Add);
        plan.ChunksToAdd.Should().HaveCount(2);
        plan.NewVersion.Should().Be(1);
    }

    [Fact]
    public async Task CommitAsync_SavesState()
    {
        var plan = new SyncPlan
        {
            DocumentId = "doc-1",
            Action = SyncAction.Add,
            ChunksToAdd = ["c1", "c2"],
            NewVersion = 1
        };

        var chunkHashes = new Dictionary<string, string>
        {
            ["c1"] = "hash1",
            ["c2"] = "hash2"
        };

        await _executor.CommitAsync(plan, chunkHashes);

        var state = await _store.GetStateAsync("doc-1");
        state.Should().NotBeNull();
        state!.Version.Should().Be(1);
        state.ChunkHashes.Should().HaveCount(2);
        state.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task PlanAndCommit_ThenPlanAgain_ReturnsSkip()
    {
        var content = Encoding.UTF8.GetBytes("Same content");
        var chunks = new List<ChunkResult>
        {
            new() { Id = "c1", Content = "Same", HeadingPath = "H1" }
        };

        // First plan + commit
        using var stream1 = new MemoryStream(content);
        var plan1 = await _executor.PlanAsync("doc-1", stream1, chunks);
        var chunkHashes = ContentHasher.ComputeChunkHashes(chunks);
        await _executor.CommitAsync(plan1, chunkHashes);

        // Save the source hash so we can verify skip
        var savedState = await _store.GetStateAsync("doc-1");

        // Update state with the correct source hash
        var stateWithHash = savedState! with
        {
            SourceHash = ContentHasher.ComputeSourceHash(content)
        };
        await _store.SaveStateAsync(stateWithHash);

        // Second plan with same content
        using var stream2 = new MemoryStream(content);
        var plan2 = await _executor.PlanAsync("doc-1", stream2, chunks);

        plan2.Action.Should().Be(SyncAction.Skip);
    }

    [Fact]
    public async Task MarkDeletedAsync_SetsDeletedFlag()
    {
        // First, add a document
        await _store.SaveStateAsync(new SyncState
        {
            DocumentId = "doc-1",
            SourceHash = "hash",
            ChunkHashes = new Dictionary<string, string> { ["c1"] = "h1" },
            Version = 1,
            LastSyncedAt = DateTimeOffset.UtcNow
        });

        var plan = await _executor.MarkDeletedAsync("doc-1");

        plan.Action.Should().Be(SyncAction.Delete);
        plan.ChunksToDelete.Should().BeEquivalentTo(["c1"]);
        plan.PreviousVersion.Should().Be(1);
        plan.NewVersion.Should().Be(2);

        var state = await _store.GetStateAsync("doc-1");
        state.Should().NotBeNull();
        state!.IsDeleted.Should().BeTrue();
        state.ChunkHashes.Should().BeEmpty();
    }

    [Fact]
    public async Task MarkDeletedAsync_NoPreviousState_CreatesDeletePlan()
    {
        var plan = await _executor.MarkDeletedAsync("non-existent");

        plan.Action.Should().Be(SyncAction.Delete);
        plan.ChunksToDelete.Should().BeEmpty();
        plan.PreviousVersion.Should().BeNull();
        plan.NewVersion.Should().Be(1);
    }
}
