using Microsoft.Extensions.DependencyInjection;

namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// Extension methods for registering MarkItDotNet security services.
/// </summary>
public static class SecurityServiceCollectionExtensions
{
    /// <summary>
    /// Adds MarkItDotNet security scanning services (<see cref="ISecurityScanner"/>).
    /// </summary>
    public static IServiceCollection AddMarkItDotNetSecurity(
        this IServiceCollection services,
        Action<SecurityScannerOptions>? configure = null)
    {
        var options = new SecurityScannerOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<ISecurityScanner, MarkdownSecurityScanner>();
        return services;
    }

    /// <summary>
    /// Adds a <see cref="PiiDetector"/> policy as a singleton <see cref="ISecurityPolicy"/>.
    /// </summary>
    public static IServiceCollection AddPiiDetector(
        this IServiceCollection services,
        Action<PiiDetectorOptions>? configure = null)
    {
        var options = new PiiDetectorOptions();
        configure?.Invoke(options);
        services.AddSingleton<ISecurityPolicy>(new PiiDetector(options));
        return services;
    }

    /// <summary>
    /// Adds a <see cref="ContentPolicyEngine"/> as a singleton <see cref="ISecurityPolicy"/>.
    /// </summary>
    public static IServiceCollection AddContentPolicyEngine(
        this IServiceCollection services,
        Action<ContentPolicyOptions>? configure = null)
    {
        var options = new ContentPolicyOptions();
        configure?.Invoke(options);
        services.AddSingleton<ISecurityPolicy>(new ContentPolicyEngine(options));
        return services;
    }

    /// <summary>
    /// Adds a <see cref="GuardrailsPolicy"/> as a singleton <see cref="ISecurityPolicy"/>.
    /// </summary>
    public static IServiceCollection AddGuardrailsPolicy(
        this IServiceCollection services,
        Action<GuardrailsPolicyOptions>? configure = null)
    {
        var options = new GuardrailsPolicyOptions();
        configure?.Invoke(options);
        services.AddSingleton<ISecurityPolicy>(new GuardrailsPolicy(options));
        return services;
    }

    /// <summary>
    /// Returns a <see cref="SecurityPoliciesBuilder"/> for fluent policy composition.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddSecurityPolicies()
    ///         .WithPiiDetector()
    ///         .WithContentPolicy(opts => opts.DenyKeywords.Add("confidential"))
    ///         .WithGuardrails()
    ///         .WithAuditLog("logs/security-audit.jsonl")
    ///         .WithChain(shortCircuit: false);
    /// </code>
    /// </example>
    public static SecurityPoliciesBuilder AddSecurityPolicies(this IServiceCollection services)
        => new(services);
}
