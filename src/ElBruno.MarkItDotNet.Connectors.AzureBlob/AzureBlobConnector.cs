// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;
using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using BlobAccessTier = Azure.Storage.Blobs.Models.AccessTier;
using BlobStates = Azure.Storage.Blobs.Models.BlobStates;
using BlobTraits = Azure.Storage.Blobs.Models.BlobTraits;
using Microsoft.Extensions.Logging;

namespace ElBruno.MarkItDotNet.Connectors.AzureBlob;

/// <summary>
/// Azure Blob Storage implementation of <see cref="IDocumentSource"/>.
/// Enumerates blobs lazily and opens content streams on demand.
/// </summary>
public sealed class AzureBlobConnector : IDocumentSource
{
    private const string AzureContainerKey = "source.azure.container";
    private const string AzureBlobNameKey = "source.azure.blobName";
    private const string AzureETagKey = "source.azure.eTag";
    private const string AzureContentTypeKey = "source.azure.contentType";
    private const string AzureContentEncodingKey = "source.azure.contentEncoding";
    private const string AzureContentLanguageKey = "source.azure.contentLanguage";
    private const string AzureAccessTierKey = "source.azure.accessTier";
    private const string AzureContentHashKey = "source.azure.contentHash";

    private readonly AzureBlobConnectorOptions _options;
    private readonly ILogger<AzureBlobConnector> _logger;
    private readonly IAzureBlobClient _blobClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobConnector"/> class.
    /// </summary>
    /// <param name="options">Connector options.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    public AzureBlobConnector(
        AzureBlobConnectorOptions options,
        ILogger<AzureBlobConnector> logger)
        : this(options, logger, null)
    {
    }

    internal AzureBlobConnector(
        AzureBlobConnectorOptions options,
        ILogger<AzureBlobConnector> logger,
        IAzureBlobClient? blobClient)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        options.Validate();

        _options = options;
        _logger = logger;
        _blobClient = blobClient ?? CreateBlobClient(options);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<SourceDocument> GetDocumentsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!await ValidateAsync(cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException(
                $"Container '{_options.ContainerName}' is not accessible.");
        }

        await foreach (var blobItem in _blobClient
                           .GetBlobItemsAsync(_options.BlobPrefix, _options.PageSize, cancellationToken)
                           .WithCancellation(cancellationToken)
                           .ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!ShouldIncludeBySize(blobItem.ContentLength))
            {
                continue;
            }

            yield return CreateSourceDocument(blobItem);
            await Task.Yield();
        }
    }

    /// <summary>
    /// Validates that the configured container is accessible.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel validation.</param>
    /// <returns>True when the container exists and can be reached; otherwise false.</returns>
    public async Task<bool> ValidateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            return await _blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogWarning(
                ex,
                "Azure Blob container validation failed for '{ContainerName}'.",
                _options.ContainerName);
            return false;
        }
    }

    /// <summary>
    /// Counts blobs that match the configured prefix and size filters.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel counting.</param>
    /// <returns>Total filtered blob count. Returns 0 when the container is not accessible.</returns>
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        if (!await ValidateAsync(cancellationToken).ConfigureAwait(false))
        {
            return 0;
        }

        int count = 0;
        await foreach (var blobItem in _blobClient
                           .GetBlobItemsAsync(_options.BlobPrefix, _options.PageSize, cancellationToken)
                           .WithCancellation(cancellationToken)
                           .ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (ShouldIncludeBySize(blobItem.ContentLength))
            {
                checked
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static IAzureBlobClient CreateBlobClient(AzureBlobConnectorOptions options)
    {
        BlobContainerClient containerClient;
        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            containerClient = new BlobContainerClient(options.ConnectionString, options.ContainerName);
        }
        else
        {
            var containerUri = new Uri(
                new Uri(options.ResolveServiceUri(), UriKind.Absolute),
                options.ContainerName);
            var blobClientOptions = new Azure.Storage.Blobs.BlobClientOptions();
            if (options.MaxRetries is int maxRetries)
            {
                blobClientOptions.Retry.MaxRetries = maxRetries;
            }

            if (options.NetworkTimeout is TimeSpan networkTimeout)
            {
                blobClientOptions.Retry.NetworkTimeout = networkTimeout;
            }

            containerClient = new BlobContainerClient(
                containerUri,
                new DefaultAzureCredential(),
                blobClientOptions);
        }

        return new AzureStorageBlobClient(containerClient);
    }

    private bool ShouldIncludeBySize(long? contentLength)
    {
        if (_options.MaxBlobSizeBytes is null)
        {
            return true;
        }

        return contentLength is null || contentLength.Value <= _options.MaxBlobSizeBytes.Value;
    }

    private SourceDocument CreateSourceDocument(BlobEntry blobItem)
    {
        var blobName = blobItem.Name;
        var fileName = Path.GetFileName(blobName.Replace('/', Path.DirectorySeparatorChar));
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = blobName;
        }

        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [SourceMetadataKeys.SourcePath] = _blobClient.GetBlobUri(blobName).ToString(),
            [SourceMetadataKeys.RelativePath] = blobName,
            [SourceMetadataKeys.FileName] = fileName,
            [SourceMetadataKeys.Extension] = Path.GetExtension(fileName),
            [SourceMetadataKeys.Depth] = GetDepth(blobName).ToString(),
            [AzureContainerKey] = _options.ContainerName,
            [AzureBlobNameKey] = blobName
        };

        if (blobItem.ContentLength is long contentLength)
        {
            metadata[SourceMetadataKeys.FileSizeBytes] = contentLength.ToString();
        }

        if (blobItem.CreatedOn is DateTimeOffset createdOn)
        {
            metadata[SourceMetadataKeys.CreatedUtc] = createdOn.UtcDateTime.ToString("O");
        }

        if (blobItem.LastModified is DateTimeOffset lastModified)
        {
            metadata[SourceMetadataKeys.LastModifiedUtc] = lastModified.UtcDateTime.ToString("O");
        }

        if (blobItem.ETag.HasValue)
        {
            metadata[AzureETagKey] = blobItem.ETag.Value.ToString();
        }

        if (!string.IsNullOrWhiteSpace(blobItem.ContentType))
        {
            metadata[AzureContentTypeKey] = blobItem.ContentType;
        }

        if (!string.IsNullOrWhiteSpace(blobItem.ContentEncoding))
        {
            metadata[AzureContentEncodingKey] = blobItem.ContentEncoding;
        }

        if (!string.IsNullOrWhiteSpace(blobItem.ContentLanguage))
        {
            metadata[AzureContentLanguageKey] = blobItem.ContentLanguage;
        }

        if (blobItem.AccessTier.HasValue)
        {
            metadata[AzureAccessTierKey] = blobItem.AccessTier.Value.ToString();
        }

        if (blobItem.ContentHash is { Length: > 0 })
        {
            metadata[AzureContentHashKey] = Convert.ToHexString(blobItem.ContentHash);
        }

        return new SourceDocument(
            id: blobName,
            name: fileName,
            source: _blobClient.GetBlobUri(blobName).ToString(),
            metadata: metadata,
            openReadAsync: cancellationToken => _blobClient.OpenReadAsync(blobName, cancellationToken));
    }

    private static int GetDepth(string blobName)
    {
        return blobName.Count(ch => ch == '/');
    }

    internal interface IAzureBlobClient
    {
        Task<bool> ExistsAsync(CancellationToken cancellationToken);
        IAsyncEnumerable<BlobEntry> GetBlobItemsAsync(string? prefix, int? pageSize, CancellationToken cancellationToken);
        Uri GetBlobUri(string blobName);
        ValueTask<Stream> OpenReadAsync(string blobName, CancellationToken cancellationToken);
    }

    internal sealed record BlobEntry(
        string Name,
        long? ContentLength,
        DateTimeOffset? CreatedOn,
        DateTimeOffset? LastModified,
        ETag? ETag,
        string? ContentType,
        string? ContentEncoding,
        string? ContentLanguage,
        BlobAccessTier? AccessTier,
        byte[]? ContentHash);

    private sealed class AzureStorageBlobClient : IAzureBlobClient
    {
        private readonly BlobContainerClient _containerClient;

        public AzureStorageBlobClient(BlobContainerClient containerClient)
        {
            _containerClient = containerClient;
        }

        public async Task<bool> ExistsAsync(CancellationToken cancellationToken)
        {
            var response = await _containerClient.ExistsAsync(cancellationToken).ConfigureAwait(false);
            return response.Value;
        }

        public async IAsyncEnumerable<BlobEntry> GetBlobItemsAsync(
            string? prefix,
            int? pageSize,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var blobPages = _containerClient.GetBlobsAsync(
                traits: BlobTraits.Metadata,
                states: BlobStates.None,
                prefix: prefix,
                cancellationToken: cancellationToken).AsPages(default, pageSize);

            await foreach (var page in blobPages.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                foreach (var blobItem in page.Values)
                {
                    yield return new BlobEntry(
                        Name: blobItem.Name,
                        ContentLength: blobItem.Properties.ContentLength,
                        CreatedOn: blobItem.Properties.CreatedOn,
                        LastModified: blobItem.Properties.LastModified,
                        ETag: blobItem.Properties.ETag,
                        ContentType: blobItem.Properties.ContentType,
                        ContentEncoding: blobItem.Properties.ContentEncoding,
                        ContentLanguage: blobItem.Properties.ContentLanguage,
                        AccessTier: blobItem.Properties.AccessTier,
                        ContentHash: blobItem.Properties.ContentHash);
                }
            }
        }

        public Uri GetBlobUri(string blobName)
        {
            return _containerClient.GetBlobClient(blobName).Uri;
        }

        public async ValueTask<Stream> OpenReadAsync(string blobName, CancellationToken cancellationToken)
        {
            return await _containerClient
                .GetBlobClient(blobName)
                .OpenReadAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
