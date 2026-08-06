// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;
using Azure;
using ElBruno.MarkItDotNet.Connectors.AzureBlob;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ElBruno.MarkItDotNet.Connectors.Tests;

public class AzureBlobConnectorTests
{
    [Fact]
    public async Task GetDocumentsAsync_AppliesPrefixAndSizeFilter_AndExtractsMetadata()
    {
        var blobs = new[]
        {
            CreateBlobItem("docs/one.md", size: 20, contentType: "text/markdown"),
            CreateBlobItem("docs/two.md", size: 400, contentType: "text/markdown"),
            CreateBlobItem("images/logo.png", size: 15, contentType: "image/png")
        };

        var fakeClient = new FakeAzureBlobClient(
            containerExists: true,
            blobItems: blobs,
            blobContents: new Dictionary<string, string>
            {
                ["docs/one.md"] = "hello markdown",
                ["docs/two.md"] = "too-large",
                ["images/logo.png"] = "img"
            });

        var connector = CreateConnector(
            options =>
            {
                options.ServiceUri = "https://markit.blob.core.windows.net";
                options.ContainerName = "documents";
                options.Prefix = "docs/";
                options.PageSize = 25;
                options.MaxBlobSizeBytes = 100;
            },
            fakeClient);

        var documents = await ToListAsync(connector.GetDocumentsAsync());

        fakeClient.ObservedPrefix.Should().Be("docs/");
        fakeClient.ObservedPageSize.Should().Be(25);
        documents.Should().ContainSingle();

        var document = documents[0];
        document.Id.Should().Be("docs/one.md");
        document.Name.Should().Be("one.md");
        document.Metadata.Should().ContainKey(SourceMetadataKeys.RelativePath).WhoseValue.Should().Be("docs/one.md");
        document.Metadata.Should().ContainKey(SourceMetadataKeys.FileSizeBytes).WhoseValue.Should().Be("20");
        document.Metadata.Should().ContainKey("source.azure.container").WhoseValue.Should().Be("documents");
        document.Metadata.Should().ContainKey("source.azure.contentType").WhoseValue.Should().Be("text/markdown");

        await using var stream = await document.OpenReadAsync();
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();
        content.Should().Be("hello markdown");
    }

    [Fact]
    public async Task ValidateAsync_ReturnsFalse_WhenContainerIsNotAccessible()
    {
        var connector = CreateConnector(
            options =>
            {
                options.ServiceUri = "https://markit.blob.core.windows.net";
                options.ContainerName = "missing";
            },
            new FakeAzureBlobClient(
                containerExists: false,
                blobItems: [],
                blobContents: new Dictionary<string, string>()));

        var isValid = await connector.ValidateAsync();

        isValid.Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_CountsOnlyFilteredBlobs()
    {
        var connector = CreateConnector(
            options =>
            {
                options.ConnectionString = "UseDevelopmentStorage=true";
                options.ContainerName = "documents";
                options.Prefix = "docs/";
                options.MaxBlobSizeBytes = 200;
            },
            new FakeAzureBlobClient(
                containerExists: true,
                blobItems:
                [
                    CreateBlobItem("docs/one.md", 100),
                    CreateBlobItem("docs/two.md", 210),
                    CreateBlobItem("notes/three.md", 20)
                ],
                blobContents: new Dictionary<string, string>()));

        var count = await connector.CountAsync();

        count.Should().Be(1);
    }

    [Fact]
    public async Task GetDocumentsAsync_ThrowsOnCancelledToken()
    {
        var connector = CreateConnector(
            options =>
            {
                options.ServiceUri = "https://markit.blob.core.windows.net";
                options.ContainerName = "documents";
            },
            new FakeAzureBlobClient(
                containerExists: true,
                blobItems: [CreateBlobItem("docs/one.md", 10)],
                blobContents: new Dictionary<string, string> { ["docs/one.md"] = "content" }));

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in connector.GetDocumentsAsync(cts.Token).ConfigureAwait(false))
            {
            }
        });
    }

    [Fact]
    public void AzureBlobConnectorOptions_Validate_AllowsAccountNameWithoutServiceUri()
    {
        var options = new AzureBlobConnectorOptions
        {
            AccountName = "markit",
            ContainerName = "documents"
        };

        options.Validate();
        options.ResolveServiceUri().Should().Be("https://markit.blob.core.windows.net");
    }

    [Fact]
    public void AzureBlobConnectorOptions_Validate_RejectsInvalidPagingAndRetryValues()
    {
        var options = new AzureBlobConnectorOptions
        {
            ServiceUri = "https://markit.blob.core.windows.net",
            ContainerName = "documents",
            PageSize = 0
        };

        Action pageSizeValidation = () => options.Validate();
        pageSizeValidation.Should().Throw<InvalidOperationException>()
            .WithMessage("*PageSize*");

        options.PageSize = 10;
        options.MaxRetries = -1;
        Action maxRetriesValidation = () => options.Validate();
        maxRetriesValidation.Should().Throw<InvalidOperationException>()
            .WithMessage("*MaxRetries*");
    }

    [Fact]
    public void AzureBlobConnectorOptions_Validate_RejectsNonPositiveNetworkTimeout()
    {
        var options = new AzureBlobConnectorOptions
        {
            ServiceUri = "https://markit.blob.core.windows.net",
            ContainerName = "documents",
            NetworkTimeout = TimeSpan.Zero
        };

        Action act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*NetworkTimeout*");
    }

    private static AzureBlobConnector CreateConnector(
        Action<AzureBlobConnectorOptions> configure,
        AzureBlobConnector.IAzureBlobClient blobClient)
    {
        var options = new AzureBlobConnectorOptions();
        configure(options);
        var logger = new TestLogger<AzureBlobConnector>();
        return new AzureBlobConnector(options, logger, blobClient);
    }

    private static AzureBlobConnector.BlobEntry CreateBlobItem(
        string name,
        long size,
        string? contentType = null)
    {
        return new AzureBlobConnector.BlobEntry(
            Name: name,
            ContentLength: size,
            CreatedOn: DateTimeOffset.UtcNow,
            LastModified: DateTimeOffset.UtcNow,
            ETag: new ETag("\"etag-value\""),
            ContentType: contentType,
            ContentEncoding: null,
            ContentLanguage: null,
            AccessTier: null,
            ContentHash: [1, 2, 3, 4]);
    }

    private static async Task<List<SourceDocument>> ToListAsync(IAsyncEnumerable<SourceDocument> source)
    {
        var documents = new List<SourceDocument>();
        await foreach (var item in source.ConfigureAwait(false))
        {
            documents.Add(item);
        }

        return documents;
    }

    private sealed class FakeAzureBlobClient : AzureBlobConnector.IAzureBlobClient
    {
        private readonly bool _containerExists;
        private readonly IReadOnlyList<AzureBlobConnector.BlobEntry> _blobItems;
        private readonly IReadOnlyDictionary<string, string> _blobContents;

        public FakeAzureBlobClient(
            bool containerExists,
            IReadOnlyList<AzureBlobConnector.BlobEntry> blobItems,
            IReadOnlyDictionary<string, string> blobContents)
        {
            _containerExists = containerExists;
            _blobItems = blobItems;
            _blobContents = blobContents;
        }

        public string? ObservedPrefix { get; private set; }
        public int? ObservedPageSize { get; private set; }

        public Task<bool> ExistsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_containerExists);
        }

        public async IAsyncEnumerable<AzureBlobConnector.BlobEntry> GetBlobItemsAsync(
            string? prefix,
            int? pageSize,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ObservedPrefix = prefix;
            ObservedPageSize = pageSize;
            foreach (var blobItem in _blobItems)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!string.IsNullOrWhiteSpace(prefix)
                    && !blobItem.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                yield return blobItem;
                await Task.Yield();
            }
        }

        public Uri GetBlobUri(string blobName)
        {
            return new Uri($"https://markit.blob.core.windows.net/documents/{blobName}");
        }

        public ValueTask<Stream> OpenReadAsync(string blobName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _blobContents.TryGetValue(blobName, out var content);
            content ??= string.Empty;
            Stream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            return ValueTask.FromResult(stream);
        }
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            return NullDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }

        private sealed class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new();
            public void Dispose()
            {
            }
        }
    }
}
