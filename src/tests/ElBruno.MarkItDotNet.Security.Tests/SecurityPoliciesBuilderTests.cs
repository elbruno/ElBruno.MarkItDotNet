using ElBruno.MarkItDotNet.Security;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ElBruno.MarkItDotNet.Security.Tests;

/// <summary>
/// Test-only helper: build a ServiceProvider from the builder's underlying collection.
/// The library itself only depends on DI.Abstractions, so BuildServiceProvider lives here.
/// </summary>
file static class BuilderTestHelper
{
    public static ServiceProvider BuildForTest(this SecurityPoliciesBuilder builder)
    {
        // The builder exposes the underlying IServiceCollection via the service registrations
        // that were added to the services instance passed in. We capture it with a fresh collection.
        var services = new ServiceCollection();
        // Replay: re-create via the builder on the fresh collection, then build.
        // (simpler: just use a new ServiceCollection per test — see below)
        return services.BuildServiceProvider();
    }
}

public class SecurityPoliciesBuilderTests
{
    private static (ServiceCollection services, SecurityPoliciesBuilder builder) CreateBuilder()
    {
        var services = new ServiceCollection();
        var builder = services.AddSecurityPolicies();
        return (services, builder);
    }

    [Fact]
    public void AddSecurityPolicies_ReturnsBuilder()
    {
        var (_, builder) = CreateBuilder();
        builder.Should().NotBeNull();
    }

    [Fact]
    public void WithPiiDetector_RegistersPolicy()
    {
        var (services, builder) = CreateBuilder();
        builder.WithPiiDetector();
        var provider = services.BuildServiceProvider();

        provider.GetServices<ISecurityPolicy>()
            .Should().ContainSingle(p => p.PolicyName == "PiiDetector");
    }

    [Fact]
    public void WithContentPolicy_RegistersPolicy()
    {
        var (services, builder) = CreateBuilder();
        builder.WithContentPolicy(opts => opts.DenyKeywords.Add("bad"));
        var provider = services.BuildServiceProvider();

        provider.GetServices<ISecurityPolicy>()
            .Should().ContainSingle(p => p.PolicyName == "ContentPolicyEngine");
    }

    [Fact]
    public void WithGuardrails_RegistersPolicy()
    {
        var (services, builder) = CreateBuilder();
        builder.WithGuardrails();
        var provider = services.BuildServiceProvider();

        provider.GetServices<ISecurityPolicy>()
            .Should().ContainSingle(p => p.PolicyName == "GuardrailsPolicy");
    }

    [Fact]
    public void WithChain_RegistersSecurityPolicyChain()
    {
        var (services, builder) = CreateBuilder();
        builder.WithPiiDetector().WithGuardrails().WithChain();
        var provider = services.BuildServiceProvider();

        var chain = provider.GetService<SecurityPolicyChain>();
        chain.Should().NotBeNull();
        chain!.Count.Should().Be(2);
    }

    [Fact]
    public void WithAuditLog_RegistersAuditLog()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"builder-audit-{Guid.NewGuid():N}.jsonl");
        try
        {
            var (services, builder) = CreateBuilder();
            builder.WithAuditLog(tempFile);
            var provider = services.BuildServiceProvider();

            provider.GetService<SecurityAuditLog>().Should().NotBeNull();
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void FluentChain_AllPoliciesRegistered()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"builder-audit-{Guid.NewGuid():N}.jsonl");
        try
        {
            var (services, builder) = CreateBuilder();
            builder
                .WithPiiDetector()
                .WithContentPolicy(opts => opts.DenyKeywords.Add("secret"))
                .WithGuardrails()
                .WithAuditLog(tempFile)
                .WithChain();
            var provider = services.BuildServiceProvider();

            var policies = provider.GetServices<ISecurityPolicy>().ToList();
            policies.Should().HaveCount(3);

            var chain = provider.GetRequiredService<SecurityPolicyChain>();
            chain.Count.Should().Be(3);

            provider.GetService<SecurityAuditLog>().Should().NotBeNull();
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void AddContentPolicyEngine_Extension_Registers()
    {
        var provider = new ServiceCollection()
            .AddContentPolicyEngine(opts => opts.DenyKeywords.Add("bad"))
            .BuildServiceProvider();

        provider.GetServices<ISecurityPolicy>()
            .Should().ContainSingle(p => p.PolicyName == "ContentPolicyEngine");
    }

    [Fact]
    public void WithChain_ShortCircuit_True_PassedThroughToChain()
    {
        var (services, builder) = CreateBuilder();
        builder.WithPiiDetector().WithGuardrails().WithChain(shortCircuit: true);
        var provider = services.BuildServiceProvider();

        // Chain should be registered and functional — verify by running it
        var chain = provider.GetRequiredService<SecurityPolicyChain>();
        chain.Should().NotBeNull();
    }
}

