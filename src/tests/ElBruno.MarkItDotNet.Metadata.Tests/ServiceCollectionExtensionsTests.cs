// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ElBruno.MarkItDotNet.Metadata.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMarkItDotNetMetadata_RegistersDocumentMetadataExtractor()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetMetadata();

        using var provider = services.BuildServiceProvider();
        provider.GetService<DocumentMetadataExtractor>().Should().NotBeNull();
    }

    [Fact]
    public void AddMarkItDotNetMetadata_RegistersIMetadataExtractor()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetMetadata();

        using var provider = services.BuildServiceProvider();
        var extractor = provider.GetService<IMetadataExtractor>();
        extractor.Should().NotBeNull()
            .And.BeOfType<CompositeMetadataExtractor>();
    }

    [Fact]
    public void AddMarkItDotNetMetadata_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddMarkItDotNetMetadata();

        result.Should().BeSameAs(services);
    }
}
