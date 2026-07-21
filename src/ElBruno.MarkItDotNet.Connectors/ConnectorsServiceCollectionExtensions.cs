// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;

namespace ElBruno.MarkItDotNet.Connectors;

/// <summary>
/// Extension methods for registering connector services with the dependency injection container.
/// </summary>
public static class ConnectorsServiceCollectionExtensions
{
    /// <summary>
    /// Adds base MarkItDotNet connector services.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddMarkItDotNetConnectors(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(NullLogger<>));
        return services;
    }

    /// <summary>
    /// Adds a file-system connector implementation of <see cref="IDocumentSource"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configure">Optional action to configure connector options.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddFileSystemConnector(
        this IServiceCollection services,
        Action<FileSystemConnectorOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMarkItDotNetConnectors();

        var options = new FileSystemConnectorOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<FileSystemConnector>();
        services.AddSingleton<IDocumentSource>(sp => sp.GetRequiredService<FileSystemConnector>());
        return services;
    }
}
