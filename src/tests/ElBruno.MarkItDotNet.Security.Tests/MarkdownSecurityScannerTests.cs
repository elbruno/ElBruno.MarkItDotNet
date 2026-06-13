using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ElBruno.MarkItDotNet.Security.Tests;

public class MarkdownSecurityScannerTests
{
    [Fact]
    public void Scan_JavaScriptLink_FlagsError()
    {
        var scanner = new MarkdownSecurityScanner();

        var result = scanner.Scan("[click](javascript:alert('xss'))");

        result.IsSafe.Should().BeFalse();
        result.Issues.Should().Contain(i => i.Code == "JAVASCRIPT_LINK" && i.Severity == SecurityIssueSeverity.Error);
    }

    [Fact]
    public void Scan_SecretLikeToken_FlagsWarning()
    {
        var scanner = new MarkdownSecurityScanner();

        var result = scanner.Scan("token_sk_AbcDefGhijk1234567890");

        result.Issues.Should().Contain(i => i.Code == "SECRET_LIKE_TOKEN");
        result.Score.Should().BeLessThan(1.0);
    }

    [Fact]
    public void Scan_CleanMarkdown_ReturnsSafeResult()
    {
        var scanner = new MarkdownSecurityScanner();

        var result = scanner.Scan("# Title\n\nRegular markdown content.");

        result.IsSafe.Should().BeTrue();
        result.Issues.Should().BeEmpty();
        result.Score.Should().Be(1.0);
    }

    [Fact]
    public void AddMarkItDotNetSecurity_RegistersScannerAndOptions()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetSecurity(options =>
        {
            options.MaxIssues = 7;
            options.DetectControlCharacters = false;
        });

        using var provider = services.BuildServiceProvider();
        provider.GetService<ISecurityScanner>().Should().NotBeNull().And.BeOfType<MarkdownSecurityScanner>();
        provider.GetService<SecurityScannerOptions>().Should().NotBeNull();
        provider.GetRequiredService<SecurityScannerOptions>().MaxIssues.Should().Be(7);
        provider.GetRequiredService<SecurityScannerOptions>().DetectControlCharacters.Should().BeFalse();
    }

    [Fact]
    public void AddMarkItDotNetSecurity_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddMarkItDotNetSecurity();

        result.Should().BeSameAs(services);
    }
}
