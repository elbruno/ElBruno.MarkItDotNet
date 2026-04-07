using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Sync.Tests.Security;

/// <summary>
/// Tests that FileSyncStateStore prevents path traversal attacks
/// via malicious document IDs. The implementation sanitizes invalid filename
/// characters (including path separators) to underscores, then validates
/// the resulting path stays within the base directory as defense-in-depth.
/// </summary>
public class PathTraversalTests : IDisposable
{
    private readonly string _testDir;
    private readonly FileSyncStateStore _store;

    public PathTraversalTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "markitdotnet-security-tests-" + Guid.NewGuid().ToString("N"));
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
    public async Task SaveStateAsync_PathTraversalDotDotSlash_SanitizedToSafeFilename()
    {
        // Path separators are invalid filename chars and get replaced with underscores
        var state = new SyncState { DocumentId = "../../etc/passwd", Version = 1 };
        await _store.SaveStateAsync(state);

        // The file should be stored safely within the base directory
        var files = Directory.GetFiles(_testDir, "*.json");
        files.Should().ContainSingle();
        files[0].Should().StartWith(_testDir);
        // Filename should NOT contain path separators
        var fileName = Path.GetFileName(files[0]);
        fileName.Should().NotContain("/");
        fileName.Should().NotContain("\\");
    }

    [Fact]
    public async Task SaveStateAsync_PathTraversalBackslash_SanitizedToSafeFilename()
    {
        var state = new SyncState { DocumentId = @"..\..\..\..\windows\system32\config", Version = 1 };
        await _store.SaveStateAsync(state);

        var files = Directory.GetFiles(_testDir, "*.json");
        files.Should().ContainSingle();
        files[0].Should().StartWith(_testDir);
    }

    [Fact]
    public async Task GetStateAsync_TraversalAttempt_FileStaysInBaseDir()
    {
        var state = new SyncState { DocumentId = "../../../important-file", Version = 1 };
        await _store.SaveStateAsync(state);

        // Verify no files were created outside the base directory
        var parentDir = Directory.GetParent(_testDir)!.FullName;
        var parentFiles = Directory.GetFiles(parentDir, "important-file*");
        parentFiles.Should().BeEmpty("path traversal should not create files outside base directory");
    }

    [Fact]
    public async Task RoundTrip_WithTraversalDocumentId_RetrievesCorrectly()
    {
        var state = new SyncState { DocumentId = "../../etc/passwd", Version = 42 };
        await _store.SaveStateAsync(state);

        var retrieved = await _store.GetStateAsync("../../etc/passwd");

        retrieved.Should().NotBeNull();
        retrieved!.DocumentId.Should().Be("../../etc/passwd");
        retrieved.Version.Should().Be(42);
    }

    [Fact]
    public async Task GetStateAsync_NormalDocumentId_DoesNotThrow()
    {
        var act = () => _store.GetStateAsync("doc-123");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetStateAsync_DocumentIdWithDashes_Works()
    {
        var state = new SyncState { DocumentId = "my-document-2024", Version = 1 };
        await _store.SaveStateAsync(state);

        var retrieved = await _store.GetStateAsync("my-document-2024");

        retrieved.Should().NotBeNull();
        retrieved!.DocumentId.Should().Be("my-document-2024");
    }

    [Fact]
    public async Task GetStateAsync_DocumentIdWithSpecialChars_Sanitized()
    {
        var state = new SyncState { DocumentId = "doc:with<special>chars", Version = 1 };
        await _store.SaveStateAsync(state);

        var retrieved = await _store.GetStateAsync("doc:with<special>chars");

        retrieved.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveStateAsync_AbsolutePathDocumentId_StaysInBaseDir()
    {
        var maliciousState = new SyncState
        {
            DocumentId = @"C:\windows\system32\config\sam",
            Version = 1
        };

        // Colons and backslashes are invalid filename chars → sanitized to underscores
        await _store.SaveStateAsync(maliciousState);

        var files = Directory.GetFiles(_testDir, "*.json");
        files.Should().AllSatisfy(f =>
            f.Should().StartWith(_testDir));
    }

    [Fact]
    public async Task DeleteStateAsync_TraversalAttempt_OnlyDeletesWithinBaseDir()
    {
        // Save and then delete with traversal doc ID
        var state = new SyncState { DocumentId = "../../etc/shadow", Version = 1 };
        await _store.SaveStateAsync(state);

        await _store.DeleteStateAsync("../../etc/shadow");

        var files = Directory.GetFiles(_testDir, "*.json");
        files.Should().BeEmpty();
    }
}
