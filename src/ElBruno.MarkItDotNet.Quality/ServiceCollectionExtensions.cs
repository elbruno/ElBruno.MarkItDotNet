// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;

namespace ElBruno.MarkItDotNet.Quality;

/// <summary>
/// Extension methods for registering quality analysis services with the dependency injection container.
/// </summary>
public static class QualityServiceCollectionExtensions
{
    /// <summary>
    /// Adds MarkItDotNet quality analysis services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configure">An optional action to configure <see cref="QualityAnalyzerOptions"/>.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddMarkItDotNetQuality(
        this IServiceCollection services,
        Action<QualityAnalyzerOptions>? configure = null)
    {
        var options = new QualityAnalyzerOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddSingleton<IQualityAnalyzer, DocumentQualityAnalyzer>();
        return services;
    }
}
