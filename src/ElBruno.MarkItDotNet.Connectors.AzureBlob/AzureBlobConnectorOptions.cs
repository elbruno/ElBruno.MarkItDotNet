// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Connectors.AzureBlob;

/// <summary>
/// Configuration options for <see cref="AzureBlobConnector"/>.
/// </summary>
public sealed class AzureBlobConnectorOptions
{
    /// <summary>
    /// Gets or sets the Azure Blob Storage connection string.
    /// When omitted, <see cref="ServiceUri"/> with <see cref="Azure.Identity.DefaultAzureCredential"/> is used.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the storage account name (for example, <c>myaccount</c>).
    /// Used to build <see cref="ServiceUri"/> when one is not provided.
    /// </summary>
    public string? AccountName { get; set; }

    /// <summary>
    /// Gets or sets the Azure Blob service URI (for example, https://myaccount.blob.core.windows.net).
    /// Required when <see cref="ConnectionString"/> is not provided.
    /// </summary>
    public string? ServiceUri { get; set; }

    /// <summary>
    /// Gets or sets the container name to enumerate.
    /// </summary>
    public string ContainerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional blob prefix filter.
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// Gets or sets an optional blob prefix filter.
    /// Alias for <see cref="Prefix"/>.
    /// </summary>
    public string? BlobPrefix
    {
        get => Prefix;
        set => Prefix = value;
    }

    /// <summary>
    /// Gets or sets the number of blobs fetched per page when listing.
    /// Null lets the SDK decide.
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    /// Gets or sets the maximum blob size in bytes.
    /// Blobs larger than this value are skipped. Null disables the limit.
    /// </summary>
    public long? MaxBlobSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the max retry attempts for transient storage operations.
    /// Null keeps SDK defaults.
    /// </summary>
    public int? MaxRetries { get; set; }

    /// <summary>
    /// Gets or sets the network timeout for storage operations.
    /// Null keeps SDK defaults.
    /// </summary>
    public TimeSpan? NetworkTimeout { get; set; }

    /// <summary>
    /// Validates options and throws if required values are missing or invalid.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ContainerName))
        {
            throw new InvalidOperationException($"{nameof(ContainerName)} is required.");
        }

        if (MaxBlobSizeBytes is < 0)
        {
            throw new InvalidOperationException($"{nameof(MaxBlobSizeBytes)} must be greater than or equal to zero.");
        }

        if (PageSize is <= 0)
        {
            throw new InvalidOperationException($"{nameof(PageSize)} must be greater than zero.");
        }

        if (MaxRetries is < 0)
        {
            throw new InvalidOperationException($"{nameof(MaxRetries)} must be greater than or equal to zero.");
        }

        if (NetworkTimeout.HasValue && NetworkTimeout.Value <= TimeSpan.Zero)
        {
            throw new InvalidOperationException($"{nameof(NetworkTimeout)} must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            if (string.IsNullOrWhiteSpace(ServiceUri) && string.IsNullOrWhiteSpace(AccountName))
            {
                throw new InvalidOperationException(
                    $"{nameof(ConnectionString)}, {nameof(ServiceUri)} or {nameof(AccountName)} is required.");
            }

            var resolvedServiceUri = ResolveServiceUri();
            if (!Uri.TryCreate(resolvedServiceUri, UriKind.Absolute, out _))
            {
                throw new InvalidOperationException($"{nameof(ServiceUri)} must be a valid absolute URI.");
            }
        }
    }

    internal string ResolveServiceUri()
    {
        if (!string.IsNullOrWhiteSpace(ServiceUri))
        {
            return ServiceUri;
        }

        if (!string.IsNullOrWhiteSpace(AccountName))
        {
            return $"https://{AccountName}.blob.core.windows.net";
        }

        throw new InvalidOperationException(
            $"{nameof(ServiceUri)} or {nameof(AccountName)} must be set when {nameof(ConnectionString)} is not provided.");
    }
}
