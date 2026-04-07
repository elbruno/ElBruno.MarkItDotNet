namespace ElBruno.MarkItDotNet;

/// <summary>
/// Configuration options for the MarkItDotNet library.
/// </summary>
public class MarkItDotNetOptions
{
    /// <summary>
    /// Enables OCR support for image and scanned-PDF conversion. Default is false (v2 feature).
    /// </summary>
    public bool EnableOcr { get; set; }

    /// <summary>
    /// Maximum file size in bytes. Files exceeding this limit will be rejected.
    /// Default is 100 MB. Set to 0 to disable the limit.
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 100 * 1024 * 1024; // 100 MB
}
