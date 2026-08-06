using Microsoft.Extensions.DependencyInjection;

namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// Fluent builder returned by <see cref="SecurityServiceCollectionExtensions.AddSecurityPolicies"/>.
/// Allows chaining multiple policies and registering a <see cref="SecurityAuditLog"/>.
/// </summary>
public sealed class SecurityPoliciesBuilder
{
    private readonly IServiceCollection _services;

    internal SecurityPoliciesBuilder(IServiceCollection services) => _services = services;

    /// <summary>Adds a <see cref="PiiDetector"/> to the policy chain.</summary>
    public SecurityPoliciesBuilder WithPiiDetector(Action<PiiDetectorOptions>? configure = null)
    {
        var opts = new PiiDetectorOptions();
        configure?.Invoke(opts);
        _services.AddSingleton<ISecurityPolicy>(new PiiDetector(opts));
        return this;
    }

    /// <summary>Adds a <see cref="ContentPolicyEngine"/> to the policy chain.</summary>
    public SecurityPoliciesBuilder WithContentPolicy(Action<ContentPolicyOptions>? configure = null)
    {
        var opts = new ContentPolicyOptions();
        configure?.Invoke(opts);
        _services.AddSingleton<ISecurityPolicy>(new ContentPolicyEngine(opts));
        return this;
    }

    /// <summary>Adds a <see cref="GuardrailsPolicy"/> to the policy chain.</summary>
    public SecurityPoliciesBuilder WithGuardrails(Action<GuardrailsPolicyOptions>? configure = null)
    {
        var opts = new GuardrailsPolicyOptions();
        configure?.Invoke(opts);
        _services.AddSingleton<ISecurityPolicy>(new GuardrailsPolicy(opts));
        return this;
    }

    /// <summary>
    /// Registers a <see cref="SecurityAuditLog"/> writing to <paramref name="filePath"/>.
    /// </summary>
    public SecurityPoliciesBuilder WithAuditLog(string filePath)
    {
        _services.AddSingleton(new SecurityAuditLog(filePath));
        return this;
    }

    /// <summary>
    /// Registers a <see cref="SecurityPolicyChain"/> that wraps all
    /// <see cref="ISecurityPolicy"/> instances registered so far.
    /// </summary>
    /// <param name="shortCircuit">Stop chain on first failing policy (default: false).</param>
    public SecurityPoliciesBuilder WithChain(bool shortCircuit = false)
    {
        _services.AddSingleton(sp =>
        {
            var policies = sp.GetServices<ISecurityPolicy>();
            return new SecurityPolicyChain(policies, shortCircuit);
        });
        return this;
    }
}
