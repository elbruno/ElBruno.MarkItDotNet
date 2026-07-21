using ElBruno.MarkItDotNet.Security;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Security.Tests;

public class SecurityAuditLogTests : IDisposable
{
    private readonly string _tempFile;

    public SecurityAuditLogTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), $"audit-test-{Guid.NewGuid():N}.jsonl");
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile)) File.Delete(_tempFile);
    }

    [Fact]
    public async Task AppendAsync_WritesEntry_ToFile()
    {
        var log = new SecurityAuditLog(_tempFile);
        await log.AppendAsync("test.md", "TestPolicy", PolicyResult.Pass());

        File.Exists(_tempFile).Should().BeTrue();
        var lines = await File.ReadAllLinesAsync(_tempFile);
        lines.Should().HaveCount(1);
        lines[0].Should().Contain("\"passed\":true");
    }

    [Fact]
    public async Task AppendAsync_MultipleEntries_EachOnOwnLine()
    {
        var log = new SecurityAuditLog(_tempFile);
        await log.AppendAsync("doc1.md", "Policy1", PolicyResult.Pass());
        await log.AppendAsync("doc2.md", "Policy2", PolicyResult.Pass());

        var lines = (await File.ReadAllLinesAsync(_tempFile))
            .Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
        lines.Should().HaveCount(2);
    }

    [Fact]
    public async Task AppendAsync_FailedResult_RecordsViolations()
    {
        var log = new SecurityAuditLog(_tempFile);
        var violations = new List<PolicyViolation>
        {
            new("PII_EMAIL", "Email found", 3, "Redact it.")
        };
        var result = PolicyResult.Fail(violations);

        await log.AppendAsync("secret.md", "PiiDetector", result);

        var text = await File.ReadAllTextAsync(_tempFile);
        text.Should().Contain("PII_EMAIL");
        text.Should().Contain("\"passed\":false");
        text.Should().Contain("\"violationCount\":1");
    }

    [Fact]
    public async Task AppendAsync_RecordsRedactionFlag_WhenRedacted()
    {
        var log = new SecurityAuditLog(_tempFile);
        var result = PolicyResult.Fail(
            [new PolicyViolation("PII_EMAIL", "Email found")],
            redactedContent: "Contact: [REDACTED]");

        await log.AppendAsync("doc.md", "PiiDetector", result);

        var text = await File.ReadAllTextAsync(_tempFile);
        text.Should().Contain("\"redactionApplied\":true");
    }

    [Fact]
    public async Task ReadAllAsync_ReturnsAllEntries()
    {
        var log = new SecurityAuditLog(_tempFile);
        await log.AppendAsync("a.md", "PolicyA", PolicyResult.Pass());
        await log.AppendAsync("b.md", "PolicyB", PolicyResult.Pass());
        await log.AppendAsync("c.md", "PolicyC", PolicyResult.Pass());

        var entries = await log.ReadAllAsync();

        entries.Should().HaveCount(3);
        entries[0].Source.Should().Be("a.md");
        entries[1].Policy.Should().Be("PolicyB");
    }

    [Fact]
    public async Task ReadAllAsync_EmptyFile_ReturnsEmptyList()
    {
        var log = new SecurityAuditLog(_tempFile);
        var entries = await log.ReadAllAsync();

        entries.Should().BeEmpty();
    }

    [Fact]
    public async Task ReadAllAsync_NonExistentFile_ReturnsEmptyList()
    {
        var log = new SecurityAuditLog(_tempFile + ".missing");
        var entries = await log.ReadAllAsync();

        entries.Should().BeEmpty();
    }

    [Fact]
    public async Task Entry_HasTimestamp()
    {
        var before = DateTimeOffset.UtcNow;
        var log = new SecurityAuditLog(_tempFile);
        await log.AppendAsync("doc.md", "Policy", PolicyResult.Pass());
        var after = DateTimeOffset.UtcNow;

        var entries = await log.ReadAllAsync();
        entries[0].Timestamp.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public async Task CreatesDirectory_IfMissing()
    {
        var deepPath = Path.Combine(Path.GetTempPath(), $"audit-sub-{Guid.NewGuid():N}", "nested", "audit.jsonl");
        try
        {
            var log = new SecurityAuditLog(deepPath);
            await log.AppendAsync("doc.md", "Policy", PolicyResult.Pass());
            File.Exists(deepPath).Should().BeTrue();
        }
        finally
        {
            if (File.Exists(deepPath)) File.Delete(deepPath);
            var dir = Path.GetDirectoryName(Path.GetDirectoryName(deepPath));
            if (dir is not null && Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }
    }
}
