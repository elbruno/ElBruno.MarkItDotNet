using ElBruno.MarkItDotNet.Security;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Security.Tests;

public class GuardrailsPolicyTests
{
    [Fact]
    public async Task CleanContent_Passes()
    {
        var sut = new GuardrailsPolicy();
        var result = await sut.EvaluateAsync("Short document.");

        result.Passed.Should().BeTrue();
        result.Violations.Should().BeEmpty();
    }

    [Fact]
    public async Task ContentExceedsMaxLength_Fails()
    {
        var opts = new GuardrailsPolicyOptions { MaxContentLength = 10 };
        var sut = new GuardrailsPolicy(opts);
        var result = await sut.EvaluateAsync("This content is definitely longer than ten characters.");

        result.Passed.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.RuleName == "GUARDRAIL_CONTENT_LENGTH");
    }

    [Fact]
    public async Task ContentAtExactMaxLength_Passes()
    {
        var opts = new GuardrailsPolicyOptions { MaxContentLength = 5 };
        var sut = new GuardrailsPolicy(opts);
        var result = await sut.EvaluateAsync("hello");

        result.Passed.Should().BeTrue();
    }

    [Fact]
    public async Task ContentExceedsMaxLines_Fails()
    {
        var opts = new GuardrailsPolicyOptions { MaxLineCount = 3 };
        var sut = new GuardrailsPolicy(opts);
        var content = "line1\nline2\nline3\nline4";

        var result = await sut.EvaluateAsync(content);

        result.Passed.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.RuleName == "GUARDRAIL_LINE_COUNT");
    }

    [Fact]
    public async Task ContentAtExactMaxLines_Passes()
    {
        var opts = new GuardrailsPolicyOptions { MaxLineCount = 3 };
        var sut = new GuardrailsPolicy(opts);
        var content = "line1\nline2\nline3";

        var result = await sut.EvaluateAsync(content);

        result.Passed.Should().BeTrue();
    }

    [Fact]
    public async Task BothLimitsViolated_BothViolationsReturned()
    {
        var opts = new GuardrailsPolicyOptions { MaxContentLength = 5, MaxLineCount = 1 };
        var sut = new GuardrailsPolicy(opts);
        var result = await sut.EvaluateAsync("line1\nline2");

        result.Passed.Should().BeFalse();
        result.Violations.Should().HaveCount(2);
        result.Violations.Select(v => v.RuleName).Should()
            .Contain("GUARDRAIL_CONTENT_LENGTH")
            .And.Contain("GUARDRAIL_LINE_COUNT");
    }

    [Fact]
    public async Task EmptyContent_Passes()
    {
        var sut = new GuardrailsPolicy();
        var result = await sut.EvaluateAsync(string.Empty);

        result.Passed.Should().BeTrue();
    }

    [Fact]
    public async Task ViolationIncludesSuggestedAction()
    {
        var opts = new GuardrailsPolicyOptions { MaxContentLength = 5 };
        var sut = new GuardrailsPolicy(opts);
        var result = await sut.EvaluateAsync("This is too long");

        result.Violations[0].SuggestedAction.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void PolicyName_IsGuardrailsPolicy()
    {
        new GuardrailsPolicy().PolicyName.Should().Be("GuardrailsPolicy");
    }
}
