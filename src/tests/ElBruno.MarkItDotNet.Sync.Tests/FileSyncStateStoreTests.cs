// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Sync.Tests;

public class FileSyncStateStoreTests : IDisposable
{
    private readonly string _testDir;
    private readonly FileSyncStateStore _store;

    public FileSyncStateStoreTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "markitdotnet-sync-tests-" + Guid.NewGuid().ToString("N"));
        _store = new FileSyncStateStore(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

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
            ChunkHashes = new Dictionary<string, string> { ["c1"] = "h1", ["c2"] = "h2" },
            Version = 3,
            LastSyncedAt = DateTimeOffset.Parse("2025-01-15T10:30:00Z"),
            IsDeleted = false,
            Metadata = new Dictionary<string, string> { ["format"] = "markdown" }
        };

        await _store.SaveStateAsync(state);
        var retrieved = await _store.GetStateAsync("doc-1");

        retrieved.Should().NotBeNull();
        retrieved!.DocumentId.Should().Be("doc-1");
        retrieved.SourceHash.Should().Be("abc123");
        retrieved.ChunkHashes.Should().HaveCount(2);
        retrieved.Version.Should().Be(3);
        retrieved.IsDeleted.Should().BeFalse();
        retrieved.Metadata.Should().ContainKey("format");
    }

    [Fact]
    public async Task SaveStateAsync_CreatesJsonFile()
    {
        await _store.SaveStateAsync(new SyncState { DocumentId = "doc-1", Version = 1 });

        var files = Directory.GetFiles(_testDir, "*.json");
        files.Should().HaveCount(1);
    }

    [Fact]
    public async Task SaveStateAsync_Overwrites()
    {
        await _store.SaveStateAsync(new SyncState { DocumentId = "doc-1", Version = 1 });
        await _store.SaveStateAsync(new SyncState { DocumentId = "doc-1", Version = 2 });

        var retrieved = await _store.GetStateAsync("doc-1");
        retrieved!.Version.Should().Be(2);
    }

    [Fact]
    public async Task DeleteStateAsync_RemovesFile()
    {
        await _store.SaveStateAsync(new SyncState { DocumentId = "doc-1", Version = 1 });
        await _store.DeleteStateAsync("doc-1");

        var result = await _store.GetStateAsync("doc-1");
        result.Should().BeNull();

        var files = Directory.GetFiles(_testDir, "*.json");
        files.Should().BeEmpty();
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
        await _store.SaveStateAsync(new SyncState { DocumentId = "doc-2", Version = 2 });

        var all = await _store.GetAllStatesAsync();

        all.Should().HaveCount(2);
        all.Select(s => s.DocumentId).Should().BeEquivalentTo(["doc-1", "doc-2"]);
    }

    [Fact]
    public async Task GetAllStatesAsync_EmptyDir_ReturnsEmptyList()
    {
        var all = await _store.GetAllStatesAsync();
        all.Should().BeEmpty();
    }
}
