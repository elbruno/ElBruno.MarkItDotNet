// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;

namespace ElBruno.MarkItDotNet.Chunking;

/// <summary>
/// Extension methods for registering chunking services with the dependency injection container.
/// </summary>
public static class ChunkingServiceCollectionExtensions
{
    /// <summary>
    /// Adds MarkItDotNet chunking services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configure">An optional action to configure <see cref="ChunkingOptions"/>.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddMarkItDotNetChunking(
        this IServiceCollection services,
        Action<ChunkingOptions>? configure = null)
    {
        var options = new ChunkingOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddSingleton<IChunkingStrategy, HeadingBasedChunker>();
        // Register all strategies as named singletons
        services.AddSingleton<HeadingBasedChunker>();
        services.AddSingleton<ParagraphBasedChunker>();
        services.AddSingleton<TokenAwareChunker>();
        return services;
    }
}
