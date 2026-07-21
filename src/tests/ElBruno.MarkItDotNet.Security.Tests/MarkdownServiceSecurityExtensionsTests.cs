using ElBruno.MarkItDotNet.Connectors;
using ElBruno.MarkItDotNet.Security;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ElBruno.MarkItDotNet.Security.Tests;

public class MarkdownServiceSecurityExtensionsTests : IDisposable
{
    private readonly string _tempDir;
    private readonly MarkdownService _service;

    public MarkdownServiceSecurityExtensionsTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"sec-ext-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        var services = new ServiceCollection().AddMarkItDotNet();
        _service = services.BuildServiceProvider().GetRequiredService<MarkdownService>();
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    // --- stream overload ---

    [Fact]
    public async Task ConvertWithPolicy_Stream_CleanContent_IsClean()
    {
        var content = "# Hello\n\nThis is a clean document.";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var policy = new PiiDetector();

        var result = await _service.ConvertWithPolicyAsync(stream, ".txt", policy);

        result.Conversion.Success.Should().BeTrue();
        result.Policy.Passed.Should().BeTrue();
        result.IsClean.Should().BeTrue();
        result.EffectiveMarkdown.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ConvertWithPolicy_Stream_PiiContent_PolicyFails()
    {
        var content = "Contact: user@example.com for support.";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var policy = new PiiDetector();

        var result = await _service.ConvertWithPolicyAsync(stream, ".txt", policy);

        result.Conversion.Success.Should().BeTrue();
        result.Policy.Passed.Should().BeFalse();
        result.IsClean.Should().BeFalse();
        result.Policy.Violations.Should().Contain(v => v.RuleName == "PII_EMAIL");
    }

    [Fact]
    public async Task ConvertWithPolicy_Stream_Redaction_EffectiveMarkdownIsRedacted()
    {
        var content = "Email: user@example.com";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var policy = new PiiDetector(new PiiDetectorOptions { EnableRedaction = true });

        var result = await _service.ConvertWithPolicyAsync(stream, ".txt", policy);

        result.EffectiveMarkdown.Should().NotContain("user@example.com");
        result.EffectiveMarkdown.Should().Contain("[REDACTED]");
    }

    [Fact]
    public async Task ConvertWithPolicy_Stream_UnsupportedFormat_ConversionFails_PolicyPasses()
    {
        using var stream = new MemoryStream([0x00, 0x01]);
        var policy = new PiiDetector();

        // .xyz is not a registered converter
        var result = await _service.ConvertWithPolicyAsync(stream, ".xyz", policy);

        result.Conversion.Success.Should().BeFalse();
        result.Policy.Passed.Should().BeTrue(); // skipped because conversion failed
        result.IsClean.Should().BeFalse();      // IsClean = conversion.Success AND policy.Passed
    }

    // --- file path overload ---

    [Fact]
    public async Task ConvertWithPolicy_FilePath_CleanFile_IsClean()
    {
        var file = Path.Combine(_tempDir, "clean.txt");
        await File.WriteAllTextAsync(file, "# Title\n\nNo PII here.");
        var policy = new PiiDetector();

        var result = await _service.ConvertWithPolicyAsync(file, policy);

        result.IsClean.Should().BeTrue();
    }

    [Fact]
    public async Task ConvertWithPolicy_FilePath_PiiFile_PolicyFails()
    {
        var file = Path.Combine(_tempDir, "pii.txt");
        await File.WriteAllTextAsync(file, "SSN: 123-45-6789");
        var policy = new PiiDetector();

        var result = await _service.ConvertWithPolicyAsync(file, policy);

        result.Conversion.Success.Should().BeTrue();
        result.Policy.Passed.Should().BeFalse();
        result.IsClean.Should().BeFalse();
    }

    // --- policy chain ---

    [Fact]
    public async Task ConvertWithPolicy_PolicyChain_BothViolationsCollected()
    {
        var content = "secret data: user@example.com";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var chain = new SecurityPolicyChain(
            new PiiDetector(),
            new ContentPolicyEngine(new ContentPolicyOptions { DenyKeywords = ["secret"] }));

        var result = await _service.ConvertWithPolicyAsync(stream, ".txt", chain);

        result.Policy.Passed.Should().BeFalse();
        result.Policy.Violations.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    // --- EffectiveMarkdown fallback ---

    [Fact]
    public async Task EffectiveMarkdown_NoPiiRedaction_ReturnsOriginalMarkdown()
    {
        var content = "# Clean\n\nNo PII.";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var policy = new GuardrailsPolicy(); // never redacts

        var result = await _service.ConvertWithPolicyAsync(stream, ".txt", policy);

        result.EffectiveMarkdown.Should().Be(result.Conversion.Markdown);
    }
}
