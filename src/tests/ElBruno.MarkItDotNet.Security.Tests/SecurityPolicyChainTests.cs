using ElBruno.MarkItDotNet.Security;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Security.Tests;

public class SecurityPolicyChainTests
{
    [Fact]
    public async Task AllPoliciesPass_ChainPasses()
    {
        var chain = new SecurityPolicyChain(
            new GuardrailsPolicy(new GuardrailsPolicyOptions { MaxContentLength = 10_000 }),
            new ContentPolicyEngine());

        var result = await chain.EvaluateAsync("Clean content.");

        result.Passed.Should().BeTrue();
        result.Violations.Should().BeEmpty();
    }

    [Fact]
    public async Task OnePolicyFails_ChainFails_CollectsViolations()
    {
        var chain = new SecurityPolicyChain(
            new GuardrailsPolicy(new GuardrailsPolicyOptions { MaxContentLength = 5 }),
            new ContentPolicyEngine());

        var result = await chain.EvaluateAsync("This content is too long.");

        result.Passed.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.RuleName == "GUARDRAIL_CONTENT_LENGTH");
    }

    [Fact]
    public async Task MultiplePoliciesFail_AllViolationsCollected()
    {
        var chain = new SecurityPolicyChain(
            new GuardrailsPolicy(new GuardrailsPolicyOptions { MaxContentLength = 5 }),
            new ContentPolicyEngine(new ContentPolicyOptions { DenyKeywords = ["bad"] }));

        var result = await chain.EvaluateAsync("bad and long content here");

        result.Passed.Should().BeFalse();
        result.Violations.Should().HaveCount(2);
    }

    [Fact]
    public async Task ShortCircuit_StopsAfterFirstFailure()
    {
        var chain = new SecurityPolicyChain(
            shortCircuit: true,
            new GuardrailsPolicy(new GuardrailsPolicyOptions { MaxContentLength = 5 }),
            new ContentPolicyEngine(new ContentPolicyOptions { DenyKeywords = ["bad"] }));

        var result = await chain.EvaluateAsync("bad and long content here");

        result.Passed.Should().BeFalse();
        // Only the first failing policy's violations should appear
        result.Violations.Should().HaveCount(1);
        result.Violations[0].RuleName.Should().Be("GUARDRAIL_CONTENT_LENGTH");
    }

    [Fact]
    public async Task RedactionMerged_ThroughChain()
    {
        // PII detector redacts email; content policy then evaluates the redacted text
        var chain = new SecurityPolicyChain(
            new PiiDetector(new PiiDetectorOptions { EnableRedaction = true }),
            new ContentPolicyEngine(new ContentPolicyOptions { DenyKeywords = ["bad"] }));

        var result = await chain.EvaluateAsync("Contact: user@example.com");

        // PII was violated and redacted; redacted content should be present
        result.RedactedContent.Should().NotBeNull();
        result.RedactedContent!.Should().NotContain("user@example.com");
    }

    [Fact]
    public async Task NoRedaction_WhenNoPolicyRedacts()
    {
        var chain = new SecurityPolicyChain(
            new GuardrailsPolicy(new GuardrailsPolicyOptions { MaxContentLength = 5 }),
            new ContentPolicyEngine(new ContentPolicyOptions { DenyKeywords = ["bad"] }));

        var result = await chain.EvaluateAsync("bad long content");

        // Neither policy produces redacted content
        result.RedactedContent.Should().BeNull();
    }

    [Fact]
    public async Task EmptyChain_Passes()
    {
        var chain = new SecurityPolicyChain();
        var result = await chain.EvaluateAsync("anything");

        result.Passed.Should().BeTrue();
    }

    [Fact]
    public async Task Chain_MetadataIncludesPolicyCount()
    {
        var chain = new SecurityPolicyChain(
            new GuardrailsPolicy(new GuardrailsPolicyOptions { MaxContentLength = 3 }),
            new ContentPolicyEngine());

        var result = await chain.EvaluateAsync("exceeds limit");

        result.Metadata.Should().ContainKey("policyCount");
        result.Metadata!["policyCount"].Should().Be(2);
    }

    [Fact]
    public void Count_ReturnsNumberOfPolicies()
    {
        var chain = new SecurityPolicyChain(
            new PiiDetector(),
            new GuardrailsPolicy(),
            new ContentPolicyEngine());

        chain.Count.Should().Be(3);
    }

    [Fact]
    public void PolicyName_IsSecurityPolicyChain()
    {
        new SecurityPolicyChain().PolicyName.Should().Be("SecurityPolicyChain");
    }

    [Fact]
    public async Task EnumerableConstructor_WorksCorrectly()
    {
        IEnumerable<ISecurityPolicy> policies = [new PiiDetector(), new GuardrailsPolicy()];
        var chain = new SecurityPolicyChain(policies);

        var result = await chain.EvaluateAsync("clean text here");

        result.Passed.Should().BeTrue();
    }
}
