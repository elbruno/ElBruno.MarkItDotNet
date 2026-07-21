// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;

namespace ElBruno.MarkItDotNet.Connectors.AzureBlob;

/// <summary>
/// Extension methods for registering Azure Blob connector services.
/// </summary>
public static class AzureBlobServiceCollectionExtensions
{
    /// <summary>
    /// Adds an Azure Blob implementation of <see cref="IDocumentSource"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configure">Optional action to configure connector options.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddAzureBlobConnector(
        this IServiceCollection services,
        Action<AzureBlobConnectorOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMarkItDotNetConnectors();

        var options = new AzureBlobConnectorOptions();
        configure?.Invoke(options);
        options.Validate();

        services.AddSingleton(options);
        services.AddSingleton<AzureBlobConnector>();
        services.AddSingleton<IDocumentSource>(sp => sp.GetRequiredService<AzureBlobConnector>());

        return services;
    }
}
