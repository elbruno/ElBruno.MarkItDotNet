using ElBruno.MarkItDotNet.Security;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Security.Tests;

public class PiiDetectorTests
{
    // --- email ---

    [Fact]
    public async Task Email_Detected_ReturnsViolation()
    {
        var sut = new PiiDetector();
        var result = await sut.EvaluateAsync("Contact us at user@example.com for support.");

        result.Passed.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.RuleName == "PII_EMAIL");
    }

    [Fact]
    public async Task Email_Redacted_WhenRedactionEnabled()
    {
        var sut = new PiiDetector(new PiiDetectorOptions { EnableRedaction = true, RedactionMask = "[HIDDEN]" });
        var result = await sut.EvaluateAsync("Email: user@example.com");

        result.RedactedContent.Should().NotBeNull();
        result.RedactedContent!.Should().NotContain("user@example.com");
        result.RedactedContent.Should().Contain("[HIDDEN]");
    }

    [Fact]
    public async Task Email_NoRedactedContent_WhenRedactionDisabled()
    {
        var sut = new PiiDetector(new PiiDetectorOptions { EnableRedaction = false });
        var result = await sut.EvaluateAsync("Email: user@example.com");

        result.Passed.Should().BeFalse();
        result.RedactedContent.Should().BeNull();
    }

    [Fact]
    public async Task Email_Disabled_NotDetected()
    {
        var sut = new PiiDetector(new PiiDetectorOptions { DetectEmails = false });
        var result = await sut.EvaluateAsync("Contact: user@example.com");

        result.Violations.Should().NotContain(v => v.RuleName == "PII_EMAIL");
    }

    // --- SSN ---

    [Fact]
    public async Task Ssn_Detected_ReturnsViolation()
    {
        var sut = new PiiDetector();
        var result = await sut.EvaluateAsync("SSN: 123-45-6789");

        result.Passed.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.RuleName == "PII_SSN");
    }

    [Fact]
    public async Task Ssn_Redacted_WhenRedactionEnabled()
    {
        var sut = new PiiDetector();
        var result = await sut.EvaluateAsync("Your SSN is 123-45-6789.");

        result.RedactedContent.Should().NotContain("123-45-6789");
    }

    [Fact]
    public async Task Ssn_Disabled_NotDetected()
    {
        var sut = new PiiDetector(new PiiDetectorOptions { DetectSsn = false });
        var result = await sut.EvaluateAsync("SSN: 123-45-6789");

        result.Violations.Should().NotContain(v => v.RuleName == "PII_SSN");
    }

    // --- phone ---

    [Fact]
    public async Task Phone_Detected_ReturnsViolation()
    {
        var sut = new PiiDetector();
        var result = await sut.EvaluateAsync("Call us at 555-123-4567 anytime.");

        result.Passed.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.RuleName == "PII_PHONE");
    }

    [Fact]
    public async Task Phone_Disabled_NotDetected()
    {
        var sut = new PiiDetector(new PiiDetectorOptions { DetectPhones = false });
        var result = await sut.EvaluateAsync("Call 555-123-4567");

        result.Violations.Should().NotContain(v => v.RuleName == "PII_PHONE");
    }

    // --- credit card ---

    [Fact]
    public async Task CreditCard_Visa_Detected()
    {
        var sut = new PiiDetector();
        var result = await sut.EvaluateAsync("Card: 4111 1111 1111 1111");

        result.Passed.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.RuleName == "PII_CREDIT_CARD");
    }

    [Fact]
    public async Task CreditCard_Disabled_NotDetected()
    {
        var sut = new PiiDetector(new PiiDetectorOptions { DetectCreditCards = false });
        var result = await sut.EvaluateAsync("Card: 4111 1111 1111 1111");

        result.Violations.Should().NotContain(v => v.RuleName == "PII_CREDIT_CARD");
    }

    // --- API key ---

    [Fact]
    public async Task ApiKey_Detected_WithKeyEqualsPattern()
    {
        var sut = new PiiDetector();
        var result = await sut.EvaluateAsync("api_key=sk-abc1234567890123456");

        result.Passed.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.RuleName == "PII_API_KEY");
    }

    [Fact]
    public async Task ApiKey_Disabled_NotDetected()
    {
        var sut = new PiiDetector(new PiiDetectorOptions { DetectApiKeys = false });
        var result = await sut.EvaluateAsync("api_key=sk-abc1234567890123456");

        result.Violations.Should().NotContain(v => v.RuleName == "PII_API_KEY");
    }

    // --- custom patterns ---

    [Fact]
    public async Task CustomPattern_Detected_WhenMatched()
    {
        var opts = new PiiDetectorOptions
        {
            DetectEmails = false,
            DetectPhones = false,
            DetectSsn = false,
            DetectCreditCards = false,
            DetectApiKeys = false,
            CustomPatterns = [@"\bEMP\d{6}\b"]
        };
        var sut = new PiiDetector(opts);
        var result = await sut.EvaluateAsync("Employee ID: EMP123456");

        result.Passed.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.RuleName == "PII_CUSTOM");
    }

    // --- clean content ---

    [Fact]
    public async Task CleanContent_Passes()
    {
        var sut = new PiiDetector();
        var result = await sut.EvaluateAsync("# Hello World\n\nThis document contains no PII.");

        result.Passed.Should().BeTrue();
        result.Violations.Should().BeEmpty();
        result.RedactedContent.Should().BeNull();
    }

    [Fact]
    public async Task EmptyContent_Passes()
    {
        var sut = new PiiDetector();
        var result = await sut.EvaluateAsync(string.Empty);

        result.Passed.Should().BeTrue();
    }

    // --- multiple violations ---

    [Fact]
    public async Task MultiplePatterns_AllDetected()
    {
        var sut = new PiiDetector();
        var content = "Email: user@example.com\nSSN: 123-45-6789\nPhone: 555-123-4567";
        var result = await sut.EvaluateAsync(content);

        result.Passed.Should().BeFalse();
        result.Violations.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task MultiplePatterns_AllRedacted()
    {
        var sut = new PiiDetector();
        var content = "Email: user@example.com\nSSN: 123-45-6789";
        var result = await sut.EvaluateAsync(content);

        result.RedactedContent.Should().NotContain("user@example.com");
        result.RedactedContent.Should().NotContain("123-45-6789");
    }

    // --- metadata ---

    [Fact]
    public async Task Violation_IncludesMetadata()
    {
        var sut = new PiiDetector();
        var result = await sut.EvaluateAsync("user@example.com");

        result.Metadata.Should().NotBeNull();
        result.Metadata!.Should().ContainKey("violationCount");
    }

    // --- PolicyName ---

    [Fact]
    public void PolicyName_IsPiiDetector()
    {
        new PiiDetector().PolicyName.Should().Be("PiiDetector");
    }
}
