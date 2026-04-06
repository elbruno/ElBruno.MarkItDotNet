// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ElBruno.MarkItDotNet.AzureSearch.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMarkItDotNetAzureSearch_RegistersAzureSearchOptions()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetAzureSearch(options =>
        {
            options.Endpoint = "https://test.search.windows.net";
            options.IndexName = "test-index";
            options.ApiKey = "test-api-key";
        });

        using var provider = services.BuildServiceProvider();
        provider.GetService<AzureSearchOptions>().Should().NotBeNull();
    }

    [Fact]
    public void AddMarkItDotNetAzureSearch_RegistersSearchClients()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetAzureSearch(options =>
        {
            options.Endpoint = "https://test.search.windows.net";
            options.IndexName = "test-index";
            options.ApiKey = "test-api-key";
        });

        using var provider = services.BuildServiceProvider();
        provider.GetService<SearchIndexClient>().Should().NotBeNull();
        provider.GetService<SearchClient>().Should().NotBeNull();
    }

    [Fact]
    public void AddMarkItDotNetAzureSearch_RegistersISearchDocumentMapper()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetAzureSearch(options =>
        {
            options.Endpoint = "https://test.search.windows.net";
            options.IndexName = "test-index";
            options.ApiKey = "test-api-key";
        });

        using var provider = services.BuildServiceProvider();
        provider.GetService<ISearchDocumentMapper>().Should().NotBeNull()
            .And.BeOfType<DefaultSearchDocumentMapper>();
    }

    [Fact]
    public void AddMarkItDotNetAzureSearch_RegistersSearchIndexManager()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetAzureSearch(options =>
        {
            options.Endpoint = "https://test.search.windows.net";
            options.IndexName = "test-index";
            options.ApiKey = "test-api-key";
        });

        using var provider = services.BuildServiceProvider();
        provider.GetService<SearchIndexManager>().Should().NotBeNull();
    }

    [Fact]
    public void AddMarkItDotNetAzureSearch_RegistersSearchIndexUploader()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetAzureSearch(options =>
        {
            options.Endpoint = "https://test.search.windows.net";
            options.IndexName = "test-index";
            options.ApiKey = "test-api-key";
        });

        using var provider = services.BuildServiceProvider();
        provider.GetService<SearchIndexUploader>().Should().NotBeNull();
    }

    [Fact]
    public void AddMarkItDotNetAzureSearch_NullConfigure_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();

        var act = () => services.AddMarkItDotNetAzureSearch(null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("configure");
    }

    [Fact]
    public void AddMarkItDotNetAzureSearch_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddMarkItDotNetAzureSearch(options =>
        {
            options.Endpoint = "https://test.search.windows.net";
            options.IndexName = "test-index";
            options.ApiKey = "test-api-key";
        });

        result.Should().BeSameAs(services);
    }
}
