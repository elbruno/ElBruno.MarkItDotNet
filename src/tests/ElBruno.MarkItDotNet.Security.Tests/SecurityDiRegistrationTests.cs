using ElBruno.MarkItDotNet.Security;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ElBruno.MarkItDotNet.Security.Tests;

public class SecurityDiRegistrationTests
{
    [Fact]
    public void AddMarkItDotNetSecurity_RegistersISecurityScanner()
    {
        var services = new ServiceCollection()
            .AddMarkItDotNetSecurity()
            .BuildServiceProvider();

        services.GetService<ISecurityScanner>().Should().NotBeNull();
        services.GetService<ISecurityScanner>().Should().BeOfType<MarkdownSecurityScanner>();
    }

    [Fact]
    public void AddMarkItDotNetSecurity_WithConfigure_AppliesOptions()
    {
        var services = new ServiceCollection()
            .AddMarkItDotNetSecurity(opts => opts.MaxIssues = 99)
            .BuildServiceProvider();

        var opts = services.GetRequiredService<SecurityScannerOptions>();
        opts.MaxIssues.Should().Be(99);
    }

    [Fact]
    public void AddPiiDetector_RegistersISecurityPolicy()
    {
        var services = new ServiceCollection()
            .AddPiiDetector()
            .BuildServiceProvider();

        var policies = services.GetServices<ISecurityPolicy>().ToList();
        policies.Should().ContainSingle(p => p.PolicyName == "PiiDetector");
    }

    [Fact]
    public void AddPiiDetector_WithConfigure_AppliesOptions()
    {
        var services = new ServiceCollection()
            .AddPiiDetector(opts => opts.EnableRedaction = false)
            .BuildServiceProvider();

        var policy = services.GetServices<ISecurityPolicy>()
            .First(p => p.PolicyName == "PiiDetector");
        policy.Should().BeOfType<PiiDetector>();
    }

    [Fact]
    public void AddGuardrailsPolicy_RegistersISecurityPolicy()
    {
        var services = new ServiceCollection()
            .AddGuardrailsPolicy()
            .BuildServiceProvider();

        var policies = services.GetServices<ISecurityPolicy>().ToList();
        policies.Should().ContainSingle(p => p.PolicyName == "GuardrailsPolicy");
    }

    [Fact]
    public void AddGuardrailsPolicy_WithConfigure_AppliesOptions()
    {
        var services = new ServiceCollection()
            .AddGuardrailsPolicy(opts => opts.MaxContentLength = 1000)
            .BuildServiceProvider();

        var policy = services.GetServices<ISecurityPolicy>()
            .First(p => p.PolicyName == "GuardrailsPolicy");
        policy.Should().BeOfType<GuardrailsPolicy>();
    }

    [Fact]
    public void MultiplePolices_AllRegistered_ResolveAsEnumerable()
    {
        var services = new ServiceCollection()
            .AddPiiDetector()
            .AddGuardrailsPolicy()
            .BuildServiceProvider();

        var policies = services.GetServices<ISecurityPolicy>().ToList();
        policies.Should().HaveCount(2);
        policies.Select(p => p.PolicyName).Should()
            .Contain("PiiDetector")
            .And.Contain("GuardrailsPolicy");
    }
}
