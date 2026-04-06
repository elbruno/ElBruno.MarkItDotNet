// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;

namespace ElBruno.MarkItDotNet.Metadata;

/// <summary>
/// Extension methods for registering metadata extraction services with the dependency injection container.
/// </summary>
public static class MetadataServiceCollectionExtensions
{
    /// <summary>
    /// Adds MarkItDotNet metadata extraction services to the specified <see cref="IServiceCollection"/>.
    /// Registers <see cref="DocumentMetadataExtractor"/> as the default <see cref="IMetadataExtractor"/>
    /// and <see cref="CompositeMetadataExtractor"/> for enrichment pipelines.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddMarkItDotNetMetadata(this IServiceCollection services)
    {
        services.AddSingleton<DocumentMetadataExtractor>();
        services.AddSingleton<IMetadataExtractor>(sp =>
        {
            var baseExtractor = sp.GetRequiredService<DocumentMetadataExtractor>();
            var enrichers = sp.GetServices<IMetadataEnricher>();
            return new CompositeMetadataExtractor(baseExtractor, enrichers);
        });

        return services;
    }
}
