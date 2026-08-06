// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using ElBruno.MarkItDotNet.Connectors.AzureBlob;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ElBruno.MarkItDotNet.Connectors.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMarkItDotNetConnectors_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddMarkItDotNetConnectors();

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddFileSystemConnector_RegistersIDocumentSource()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "MarkItDotNet.Connectors.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempPath);

        try
        {
            var services = new ServiceCollection();
            services.AddFileSystemConnector(options => options.RootPath = tempPath);

            using var provider = services.BuildServiceProvider();
            var source = provider.GetService<IDocumentSource>();

            source.Should().NotBeNull().And.BeOfType<FileSystemConnector>();
        }
        finally
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, recursive: true);
            }
        }
    }

    [Fact]
    public void AddFileSystemConnector_WithConfigure_AppliesOptions()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "MarkItDotNet.Connectors.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempPath);

        try
        {
            var services = new ServiceCollection();
            services.AddFileSystemConnector(options =>
            {
                options.RootPath = tempPath;
                options.MaxDepth = 2;
            });

            using var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<FileSystemConnectorOptions>();

            options.RootPath.Should().Be(tempPath);
            options.MaxDepth.Should().Be(2);
        }
        finally
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, recursive: true);
            }
        }
    }

    [Fact]
    public void AddAzureBlobConnector_RegistersIDocumentSource()
    {
        var services = new ServiceCollection();
        services.AddAzureBlobConnector(options =>
        {
            options.ServiceUri = "https://markit.blob.core.windows.net";
            options.ContainerName = "documents";
        });

        using var provider = services.BuildServiceProvider();
        var source = provider.GetService<IDocumentSource>();

        source.Should().NotBeNull().And.BeOfType<AzureBlobConnector>();
    }

    [Fact]
    public void AddAzureBlobConnector_WithConfigure_AppliesOptions()
    {
        var services = new ServiceCollection();
        services.AddAzureBlobConnector(options =>
        {
            options.ServiceUri = "https://markit.blob.core.windows.net";
            options.ContainerName = "documents";
            options.Prefix = "docs/";
            options.MaxBlobSizeBytes = 1024;
        });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<AzureBlobConnectorOptions>();

        options.ServiceUri.Should().Be("https://markit.blob.core.windows.net");
        options.ContainerName.Should().Be("documents");
        options.Prefix.Should().Be("docs/");
        options.MaxBlobSizeBytes.Should().Be(1024);
    }

    [Fact]
    public void AddFileSystemAndAzureBlobConnector_RegisterBothDocumentSources()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "MarkItDotNet.Connectors.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempPath);

        try
        {
            var services = new ServiceCollection();
            services.AddFileSystemConnector(options => options.RootPath = tempPath);
            services.AddAzureBlobConnector(options =>
            {
                options.AccountName = "markit";
                options.ContainerName = "documents";
            });

            using var provider = services.BuildServiceProvider();
            var sources = provider.GetServices<IDocumentSource>().ToList();

            sources.Should().HaveCount(2);
            sources.Should().ContainSingle(source => source is FileSystemConnector);
            sources.Should().ContainSingle(source => source is AzureBlobConnector);
        }
        finally
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, recursive: true);
            }
        }
    }
}
