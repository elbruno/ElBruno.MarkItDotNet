// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Sync.Tests;

public class InMemorySyncStateStoreTests
{
    private readonly InMemorySyncStateStore _store = new();

    [Fact]
    public async Task GetStateAsync_NotFound_ReturnsNull()
    {
        var result = await _store.GetStateAsync("non-existent");
        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveAndGetStateAsync_RoundTrips()
    {
        var state = new SyncState
        {
            DocumentId = "doc-1",
            SourceHash = "abc123",
            ChunkHashes = new Dictionary<string, string> { ["c1"] = "h1" },
            Version = 1,
            LastSyncedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            Metadata = new Dictionary<string, string> { ["path"] = "/test.md" }
        };

        await _store.SaveStateAsync(state);
        var retrieved = await _store.GetStateAsync("doc-1");

        retrieved.Should().NotBeNull();
        retrieved!.DocumentId.Should().Be("doc-1");
        retrieved.SourceHash.Should().Be("abc123");
        retrieved.ChunkHashes.Should().ContainKey("c1");
        retrieved.Version.Should().Be(1);
        retrieved.Metadata.Should().ContainKey("path");
    }

    [Fact]
    public async Task SaveStateAsync_Overwrites()
    {
        var state1 = new SyncState { DocumentId = "doc-1", Version = 1 };
        var state2 = new SyncState { DocumentId = "doc-1", Version = 2 };

        await _store.SaveStateAsync(state1);
        await _store.SaveStateAsync(state2);

        var retrieved = await _store.GetStateAsync("doc-1");
        retrieved!.Version.Should().Be(2);
    }

    [Fact]
    public async Task DeleteStateAsync_RemovesState()
    {
        await _store.SaveStateAsync(new SyncState { DocumentId = "doc-1", Version = 1 });
        await _store.DeleteStateAsync("doc-1");

        var result = await _store.GetStateAsync("doc-1");
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteStateAsync_NonExistent_DoesNotThrow()
    {
        var act = () => _store.DeleteStateAsync("non-existent");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetAllStatesAsync_ReturnsAllStates()
    {
        await _store.SaveStateAsync(new SyncState { DocumentId = "doc-1", Version = 1 });
        await _store.SaveStateAsync(new SyncState { DocumentId = "doc-2", Version = 1 });

        var all = await _store.GetAllStatesAsync();

        all.Should().HaveCount(2);
        all.Select(s => s.DocumentId).Should().BeEquivalentTo(["doc-1", "doc-2"]);
    }

    [Fact]
    public async Task GetAllStatesAsync_Empty_ReturnsEmptyList()
    {
        var all = await _store.GetAllStatesAsync();
        all.Should().BeEmpty();
    }
}
