// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;

namespace ElBruno.MarkItDotNet.Sync;

/// <summary>
/// Extension methods for registering sync services with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MarkItDotNet sync services to the specified <see cref="IServiceCollection"/>.
    /// Registers <see cref="SyncExecutor"/> and optionally an <see cref="ISyncStateStore"/> implementation.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configureStore">
    /// An optional factory to provide an <see cref="ISyncStateStore"/> implementation.
    /// If not provided, <see cref="InMemorySyncStateStore"/> is used as the default.
    /// </param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddMarkItDotNetSync(
        this IServiceCollection services,
        Func<IServiceProvider, ISyncStateStore>? configureStore = null)
    {
        if (configureStore is not null)
        {
            services.AddSingleton(configureStore);
        }
        else
        {
            services.AddSingleton<ISyncStateStore, InMemorySyncStateStore>();
        }

        services.AddTransient<SyncExecutor>();

        return services;
    }
}
