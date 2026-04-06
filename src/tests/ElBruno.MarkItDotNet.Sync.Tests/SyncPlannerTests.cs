// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Sync.Tests;

public class SyncPlannerTests
{
    [Fact]
    public void ComputePlan_NoPreviousState_ReturnsAddAllChunks()
    {
        var chunkHashes = new Dictionary<string, string>
        {
            ["chunk-1"] = "hash1",
            ["chunk-2"] = "hash2"
        };

        var plan = SyncPlanner.ComputePlan("doc-1", "source-hash", chunkHashes, previousState: null);

        plan.DocumentId.Should().Be("doc-1");
        plan.Action.Should().Be(SyncAction.Add);
        plan.ChunksToAdd.Should().BeEquivalentTo(["chunk-1", "chunk-2"]);
        plan.ChunksToUpdate.Should().BeEmpty();
        plan.ChunksToDelete.Should().BeEmpty();
        plan.ChunksUnchanged.Should().BeEmpty();
        plan.PreviousVersion.Should().BeNull();
        plan.NewVersion.Should().Be(1);
    }

    [Fact]
    public void ComputePlan_SameSourceHash_ReturnsSkip()
    {
        var chunkHashes = new Dictionary<string, string>
        {
            ["chunk-1"] = "hash1",
            ["chunk-2"] = "hash2"
        };

        var previousState = new SyncState
        {
            DocumentId = "doc-1",
            SourceHash = "source-hash",
            ChunkHashes = chunkHashes,
            Version = 3,
            LastSyncedAt = DateTimeOffset.UtcNow.AddHours(-1)
        };

        var plan = SyncPlanner.ComputePlan("doc-1", "source-hash", chunkHashes, previousState);

        plan.Action.Should().Be(SyncAction.Skip);
        plan.ChunksToAdd.Should().BeEmpty();
        plan.ChunksToUpdate.Should().BeEmpty();
        plan.ChunksToDelete.Should().BeEmpty();
        plan.ChunksUnchanged.Should().BeEquivalentTo(["chunk-1", "chunk-2"]);
        plan.PreviousVersion.Should().Be(3);
        plan.NewVersion.Should().Be(3);
    }

    [Fact]
    public void ComputePlan_DifferentSourceHash_DiffsChunks()
    {
        var previousState = new SyncState
        {
            DocumentId = "doc-1",
            SourceHash = "old-hash",
            ChunkHashes = new Dictionary<string, string>
            {
                ["chunk-1"] = "hash1",
                ["chunk-2"] = "hash2",
                ["chunk-3"] = "hash3"
            },
            Version = 2
        };

        var currentChunkHashes = new Dictionary<string, string>
        {
            ["chunk-1"] = "hash1",       // unchanged
            ["chunk-2"] = "hash2-new",   // updated
            ["chunk-4"] = "hash4"        // added (chunk-3 deleted)
        };

        var plan = SyncPlanner.ComputePlan("doc-1", "new-hash", currentChunkHashes, previousState);

        plan.Action.Should().Be(SyncAction.Update);
        plan.ChunksToAdd.Should().BeEquivalentTo(["chunk-4"]);
        plan.ChunksToUpdate.Should().BeEquivalentTo(["chunk-2"]);
        plan.ChunksToDelete.Should().BeEquivalentTo(["chunk-3"]);
        plan.ChunksUnchanged.Should().BeEquivalentTo(["chunk-1"]);
        plan.PreviousVersion.Should().Be(2);
        plan.NewVersion.Should().Be(3);
    }

    [Fact]
    public void ComputePlan_AllChunksRemoved_ReturnsUpdateWithDeletesOnly()
    {
        var previousState = new SyncState
        {
            DocumentId = "doc-1",
            SourceHash = "old-hash",
            ChunkHashes = new Dictionary<string, string>
            {
                ["chunk-1"] = "hash1",
                ["chunk-2"] = "hash2"
            },
            Version = 1
        };

        var currentChunkHashes = new Dictionary<string, string>();

        var plan = SyncPlanner.ComputePlan("doc-1", "new-hash", currentChunkHashes, previousState);

        plan.Action.Should().Be(SyncAction.Update);
        plan.ChunksToAdd.Should().BeEmpty();
        plan.ChunksToUpdate.Should().BeEmpty();
        plan.ChunksToDelete.Should().BeEquivalentTo(["chunk-1", "chunk-2"]);
        plan.ChunksUnchanged.Should().BeEmpty();
    }

    [Fact]
    public void ComputePlan_AllNewChunks_ReturnsUpdateWithAddsOnly()
    {
        var previousState = new SyncState
        {
            DocumentId = "doc-1",
            SourceHash = "old-hash",
            ChunkHashes = new Dictionary<string, string>(),
            Version = 1
        };

        var currentChunkHashes = new Dictionary<string, string>
        {
            ["chunk-new"] = "hash-new"
        };

        var plan = SyncPlanner.ComputePlan("doc-1", "new-hash", currentChunkHashes, previousState);

        plan.Action.Should().Be(SyncAction.Update);
        plan.ChunksToAdd.Should().BeEquivalentTo(["chunk-new"]);
        plan.ChunksToDelete.Should().BeEmpty();
    }
}
