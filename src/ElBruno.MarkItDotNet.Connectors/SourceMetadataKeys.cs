// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Connectors;

/// <summary>
/// Standard metadata keys used by connector implementations.
/// </summary>
public static class SourceMetadataKeys
{
    /// <summary>Absolute source path for the discovered file.</summary>
    public const string SourcePath = "source.path";

    /// <summary>Relative path from connector root.</summary>
    public const string RelativePath = "source.relativePath";

    /// <summary>File name including extension.</summary>
    public const string FileName = "source.fileName";

    /// <summary>File extension with leading dot.</summary>
    public const string Extension = "source.extension";

    /// <summary>File size in bytes.</summary>
    public const string FileSizeBytes = "source.fileSizeBytes";

    /// <summary>File creation time in UTC (ISO 8601).</summary>
    public const string CreatedUtc = "source.createdUtc";

    /// <summary>File last write time in UTC (ISO 8601).</summary>
    public const string LastModifiedUtc = "source.lastModifiedUtc";

    /// <summary>Directory traversal depth relative to the connector root.</summary>
    public const string Depth = "source.depth";
}
