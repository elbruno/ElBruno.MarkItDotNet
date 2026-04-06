// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.AzureSearch.Tests;

public class AzureSearchOptionsTests
{
    [Fact]
    public void Defaults_VectorDimensionsIs1536()
    {
        var options = new AzureSearchOptions();

        options.VectorDimensions.Should().Be(1536);
    }

    [Fact]
    public void Defaults_BatchSizeIs100()
    {
        var options = new AzureSearchOptions();

        options.BatchSize.Should().Be(100);
    }

    [Fact]
    public void Defaults_EndpointIsEmpty()
    {
        var options = new AzureSearchOptions();

        options.Endpoint.Should().BeEmpty();
    }

    [Fact]
    public void Defaults_IndexNameIsEmpty()
    {
        var options = new AzureSearchOptions();

        options.IndexName.Should().BeEmpty();
    }

    [Fact]
    public void Defaults_ApiKeyIsNull()
    {
        var options = new AzureSearchOptions();

        options.ApiKey.Should().BeNull();
    }

    [Fact]
    public void Validate_MissingEndpoint_Throws()
    {
        var options = new AzureSearchOptions
        {
            IndexName = "test-index",
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Endpoint*");
    }

    [Fact]
    public void Validate_MissingIndexName_Throws()
    {
        var options = new AzureSearchOptions
        {
            Endpoint = "https://test.search.windows.net",
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*IndexName*");
    }

    [Fact]
    public void Validate_ZeroVectorDimensions_Throws()
    {
        var options = new AzureSearchOptions
        {
            Endpoint = "https://test.search.windows.net",
            IndexName = "test-index",
            VectorDimensions = 0,
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*VectorDimensions*");
    }

    [Fact]
    public void Validate_ZeroBatchSize_Throws()
    {
        var options = new AzureSearchOptions
        {
            Endpoint = "https://test.search.windows.net",
            IndexName = "test-index",
            BatchSize = 0,
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*BatchSize*");
    }

    [Fact]
    public void Validate_ValidOptions_DoesNotThrow()
    {
        var options = new AzureSearchOptions
        {
            Endpoint = "https://test.search.windows.net",
            IndexName = "test-index",
        };

        var act = () => options.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithApiKey_DoesNotThrow()
    {
        var options = new AzureSearchOptions
        {
            Endpoint = "https://test.search.windows.net",
            IndexName = "test-index",
            ApiKey = "my-api-key",
        };

        var act = () => options.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_NegativeBatchSize_Throws()
    {
        var options = new AzureSearchOptions
        {
            Endpoint = "https://test.search.windows.net",
            IndexName = "test-index",
            BatchSize = -1,
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Validate_NegativeVectorDimensions_Throws()
    {
        var options = new AzureSearchOptions
        {
            Endpoint = "https://test.search.windows.net",
            IndexName = "test-index",
            VectorDimensions = -5,
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>();
    }
}
