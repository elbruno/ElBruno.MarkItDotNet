using ElBruno.MarkItDotNet.CoreModel;
using Microsoft.Extensions.DependencyInjection;

namespace ElBruno.MarkItDotNet.DocumentIntelligence;

/// <summary>
/// Extension methods for registering Azure Document Intelligence services with the DI container.
/// </summary>
public static class DocumentIntelligenceServiceCollectionExtensions
{
    /// <summary>
    /// Adds Azure Document Intelligence converter services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configure">Action to configure <see cref="DocumentIntelligenceOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMarkItDotNetDocumentIntelligence(
        this IServiceCollection services,
        Action<DocumentIntelligenceOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new DocumentIntelligenceOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddSingleton<IStructuredConverter, DocumentIntelligenceConverter>();

        return services;
    }
}
