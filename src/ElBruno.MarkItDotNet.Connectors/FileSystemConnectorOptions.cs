// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Connectors;

/// <summary>
/// Configuration options for <see cref="FileSystemConnector"/>.
/// </summary>
public sealed class FileSystemConnectorOptions
{
    /// <summary>
    /// Gets or sets the root directory to scan.
    /// </summary>
    public string RootPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether subdirectories should be traversed.
    /// </summary>
    public bool Recursive { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum directory depth relative to <see cref="RootPath"/>.
    /// Root directory depth is 0.
    /// </summary>
    public int MaxDepth { get; set; } = int.MaxValue;

    /// <summary>
    /// Gets or sets glob patterns to include, such as <c>*.md</c> or <c>docs/**/*.md</c>.
    /// If empty, all files are included.
    /// </summary>
    public IReadOnlyList<string> IncludePatterns { get; set; } = ["*"];

    /// <summary>
    /// Gets or sets the maximum file size in bytes.
    /// Files larger than this value are skipped. Null disables the limit.
    /// </summary>
    public long? MaxFileSizeBytes { get; set; }
}
