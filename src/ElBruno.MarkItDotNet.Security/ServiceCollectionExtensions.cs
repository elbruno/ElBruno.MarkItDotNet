using Microsoft.Extensions.DependencyInjection;

namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// Extension methods for registering MarkItDotNet security services.
/// </summary>
public static class SecurityServiceCollectionExtensions
{
    /// <summary>
    /// Adds MarkItDotNet security scanning services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The same service collection for chaining.</returns>
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
}
