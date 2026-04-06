// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.CoreModel;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ElBruno.MarkItDotNet.DocumentIntelligence.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMarkItDotNetDocumentIntelligence_RegistersDocumentIntelligenceOptions()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetDocumentIntelligence(options =>
        {
            options.Endpoint = "https://test.cognitiveservices.azure.com";
            options.ApiKey = "test-api-key";
        });

        using var provider = services.BuildServiceProvider();
        provider.GetService<DocumentIntelligenceOptions>().Should().NotBeNull();
    }

    [Fact]
    public void AddMarkItDotNetDocumentIntelligence_RegistersIStructuredConverter()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetDocumentIntelligence(options =>
        {
            options.Endpoint = "https://test.cognitiveservices.azure.com";
            options.ApiKey = "test-api-key";
        });

        using var provider = services.BuildServiceProvider();
        provider.GetService<IStructuredConverter>().Should().NotBeNull()
            .And.BeOfType<DocumentIntelligenceConverter>();
    }

    [Fact]
    public void AddMarkItDotNetDocumentIntelligence_NullConfigure_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();

        var act = () => services.AddMarkItDotNetDocumentIntelligence(null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("configure");
    }

    [Fact]
    public void AddMarkItDotNetDocumentIntelligence_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddMarkItDotNetDocumentIntelligence(options =>
        {
            options.Endpoint = "https://test.cognitiveservices.azure.com";
            options.ApiKey = "test-api-key";
        });

        result.Should().BeSameAs(services);
    }
}
