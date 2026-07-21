using FluentAssertions;
using SecurityPoliciesDemoSample;
using Xunit;

namespace ElBruno.MarkItDotNet.SecurityPoliciesDemo.Tests;

public class SecurityDemoOptionsAndPolicyTests
{
    [Fact]
    public void Parse_LoadsDefaultsFromAppSettings()
    {
        using var fixture = new TempDirectoryFixture();
        File.WriteAllText(
            Path.Combine(fixture.Path, "appsettings.json"),
            """
            {
              "SecurityPoliciesDemo": {
                "OutputPath": "audit-output",
                "AuditLogPath": "audit-output/log.jsonl",
                "DryRun": true,
                "MaxContentLength": 120,
                "EnablePiiRedaction": false,
                "DenyKeywords": ["classified"]
              }
            }
            """
        );

        var options = SecurityDemoOptionsParser.Parse([], fixture.Path);

        options.OutputPath.Should().Be(Path.GetFullPath("audit-output", fixture.Path));
        options.AuditLogPath.Should().Be(Path.GetFullPath("audit-output/log.jsonl", fixture.Path));
        options.DryRun.Should().BeTrue();
        options.MaxContentLength.Should().Be(120);
        options.EnablePiiRedaction.Should().BeFalse();
        options.DenyKeywords.Should().ContainSingle().Which.Should().Be("classified");
    }

    [Fact]
    public void Parse_AppliesCommandLineOverrides()
    {
        using var fixture = new TempDirectoryFixture();

        var options = SecurityDemoOptionsParser.Parse(
            [
                "--output", "cli-output",
                "--audit-log=cli-output/audit.jsonl",
                "--max-content-length", "512",
                "--deny-keyword", "internal",
                "--dry-run"
            ],
            fixture.Path);

        options.OutputPath.Should().Be(Path.GetFullPath("cli-output", fixture.Path));
        options.AuditLogPath.Should().Be(Path.GetFullPath("cli-output/audit.jsonl", fixture.Path));
        options.MaxContentLength.Should().Be(512);
        options.DryRun.Should().BeTrue();
        options.DenyKeywords.Should().Contain("internal");
    }

    [Fact]
    public void Evaluate_DetectsViolationsAndRedactsPii()
    {
        const string markdown = "Contact jane.doe@contoso.com and call 555-123-4567. This is confidential.";
        var rules = new SecurityPolicyRules
        {
            DenyKeywords = ["confidential"],
            MaxContentLength = 500,
            EnablePiiRedaction = true,
            RedactionMask = "[MASKED]"
        };

        var result = SecurityPolicyEngine.Evaluate(markdown, rules);

        result.Passed.Should().BeFalse();
        result.Violations.Select(v => v.Code).Should().Contain(["PII_DETECTED", "DENY_KEYWORD"]);
        result.ProcessedContent.Should().Contain("[MASKED]");
        result.ProcessedContent.Should().NotContain("jane.doe@contoso.com");
        result.ProcessedContent.Should().NotContain("555-123-4567");
    }

    private sealed class TempDirectoryFixture : IDisposable
    {
        public TempDirectoryFixture()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
