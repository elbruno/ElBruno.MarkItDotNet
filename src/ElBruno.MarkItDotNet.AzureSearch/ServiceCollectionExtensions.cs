// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using Azure;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.DependencyInjection;

namespace ElBruno.MarkItDotNet.AzureSearch;

/// <summary>
/// Extension methods for registering Azure AI Search services with the dependency injection container.
/// </summary>
public static class AzureSearchServiceCollectionExtensions
{
    /// <summary>
    /// Adds MarkItDotNet Azure AI Search services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configure">An action to configure <see cref="AzureSearchOptions"/>.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddMarkItDotNetAzureSearch(
        this IServiceCollection services,
        Action<AzureSearchOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new AzureSearchOptions();
        configure(options);
        options.Validate();

        services.AddSingleton(options);

        var endpoint = new Uri(options.Endpoint);

        if (!string.IsNullOrWhiteSpace(options.ApiKey))
        {
            var credential = new AzureKeyCredential(options.ApiKey);
            services.AddSingleton(new SearchIndexClient(endpoint, credential));
            services.AddSingleton(new SearchClient(endpoint, options.IndexName, credential));
        }
        else
        {
            var credential = new DefaultAzureCredential();
            services.AddSingleton(new SearchIndexClient(endpoint, credential));
            services.AddSingleton(new SearchClient(endpoint, options.IndexName, credential));
        }

        services.AddSingleton<ISearchDocumentMapper, DefaultSearchDocumentMapper>();
        services.AddSingleton<SearchIndexManager>();
        services.AddSingleton<SearchIndexUploader>();

        return services;
    }
}
