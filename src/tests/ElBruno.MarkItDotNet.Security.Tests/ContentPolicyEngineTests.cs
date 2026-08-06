using ElBruno.MarkItDotNet.Security;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Security.Tests;

public class ContentPolicyEngineTests
{
    // --- deny keywords ---

    [Fact]
    public async Task DenyKeyword_Found_ReturnsViolation()
    {
        var sut = new ContentPolicyEngine(new ContentPolicyOptions
        {
            DenyKeywords = ["confidential"]
        });
        var result = await sut.EvaluateAsync("This document is confidential.");

        result.Passed.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.RuleName == "DENY_KEYWORD");
    }

    [Fact]
    public async Task DenyKeyword_CaseInsensitive_Detected()
    {
        var sut = new ContentPolicyEngine(new ContentPolicyOptions
        {
            DenyKeywords = ["CONFIDENTIAL"]
        });
        var result = await sut.EvaluateAsync("This is Confidential content.");

        result.Passed.Should().BeFalse();
    }

    [Fact]
    public async Task DenyKeyword_NotPresent_Passes()
    {
        var sut = new ContentPolicyEngine(new ContentPolicyOptions
        {
            DenyKeywords = ["confidential"]
        });
        var result = await sut.EvaluateAsync("This is a public document.");

        result.Passed.Should().BeTrue();
    }

    [Fact]
    public async Task MultipleDenyKeywords_AllDetected()
    {
        var sut = new ContentPolicyEngine(new ContentPolicyOptions
        {
            DenyKeywords = ["confidential", "restricted", "secret"]
        });
        var result = await sut.EvaluateAsync("This is confidential, restricted, and secret.");

        result.Violations.Should().HaveCount(3);
    }

    [Fact]
    public async Task DenyKeyword_ShortCircuit_StopsAfterFirst()
    {
        var sut = new ContentPolicyEngine(new ContentPolicyOptions
        {
            DenyKeywords = ["alpha", "beta", "gamma"],
            ShortCircuit = true
        });
        var result = await sut.EvaluateAsync("alpha beta gamma");

        result.Passed.Should().BeFalse();
        result.Violations.Should().HaveCount(1);
    }

    // --- blocked domains ---

    [Fact]
    public async Task BlockedDomain_InUrl_ReturnsViolation()
    {
        var sut = new ContentPolicyEngine(new ContentPolicyOptions
        {
            BlockedDomains = ["malicious.com"]
        });
        var result = await sut.EvaluateAsync("See [link](https://malicious.com/page) for details.");

        result.Passed.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.RuleName == "BLOCKED_DOMAIN");
    }

    [Fact]
    public async Task BlockedDomain_NotInContent_Passes()
    {
        var sut = new ContentPolicyEngine(new ContentPolicyOptions
        {
            BlockedDomains = ["malicious.com"]
        });
        var result = await sut.EvaluateAsync("Visit https://trusted.com/page for info.");

        result.Passed.Should().BeTrue();
    }

    [Fact]
    public async Task BlockedDomain_CaseInsensitive()
    {
        var sut = new ContentPolicyEngine(new ContentPolicyOptions
        {
            BlockedDomains = ["MALICIOUS.COM"]
        });
        var result = await sut.EvaluateAsync("See https://malicious.com/path");

        result.Passed.Should().BeFalse();
    }

    // --- combined ---

    [Fact]
    public async Task DenyKeywordAndBlockedDomain_BothDetected()
    {
        var sut = new ContentPolicyEngine(new ContentPolicyOptions
        {
            DenyKeywords = ["secret"],
            BlockedDomains = ["evil.com"]
        });
        var result = await sut.EvaluateAsync("secret data at https://evil.com/api");

        result.Violations.Should().HaveCount(2);
    }

    // --- clean ---

    [Fact]
    public async Task EmptyConfig_AlwaysPasses()
    {
        var sut = new ContentPolicyEngine();
        var result = await sut.EvaluateAsync("Any content here.");

        result.Passed.Should().BeTrue();
    }

    [Fact]
    public async Task EmptyContent_Passes()
    {
        var sut = new ContentPolicyEngine(new ContentPolicyOptions { DenyKeywords = ["bad"] });
        var result = await sut.EvaluateAsync(string.Empty);

        result.Passed.Should().BeTrue();
    }

    [Fact]
    public void PolicyName_IsContentPolicyEngine()
    {
        new ContentPolicyEngine().PolicyName.Should().Be("ContentPolicyEngine");
    }

    [Fact]
    public async Task ViolationIncludesLineNumber()
    {
        var sut = new ContentPolicyEngine(new ContentPolicyOptions
        {
            DenyKeywords = ["secret"]
        });
        var result = await sut.EvaluateAsync("line1\nline2\nsecret here\nline4");

        result.Violations[0].LineNumber.Should().Be(3);
    }
}
