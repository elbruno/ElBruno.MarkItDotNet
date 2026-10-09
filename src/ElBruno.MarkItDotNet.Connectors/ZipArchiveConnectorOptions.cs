// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Connectors;

/// <summary>
/// Configuration options for <see cref="ZipArchiveConnector"/>.
/// </summary>
public sealed class ZipArchiveConnectorOptions
{
    /// <summary>
    /// Gets or sets the path to the ZIP archive.
    /// </summary>
    public string ArchivePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum compressed archive size in bytes.
    /// </summary>
    public long MaxArchiveSizeBytes { get; set; } = 100 * 1024 * 1024;

    /// <summary>
    /// Gets or sets the maximum uncompressed size of a single entry in bytes.
    /// Larger entries are skipped.
    /// </summary>
    public long MaxEntrySizeBytes { get; set; } = 25 * 1024 * 1024;

    /// <summary>
    /// Gets or sets the maximum combined uncompressed size of entries in bytes.
    /// </summary>
    public long MaxTotalUncompressedBytes { get; set; } = 250 * 1024 * 1024;

    /// <summary>
    /// Gets or sets the maximum number of entries in the archive, including directories.
    /// </summary>
    public int MaxEntries { get; set; } = 10_000;
}
