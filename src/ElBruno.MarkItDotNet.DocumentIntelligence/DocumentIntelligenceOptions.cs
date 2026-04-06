namespace ElBruno.MarkItDotNet.DocumentIntelligence;

/// <summary>
/// Configuration options for the Azure Document Intelligence converter.
/// </summary>
public class DocumentIntelligenceOptions
{
    /// <summary>
    /// The Azure Document Intelligence endpoint URL.
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// The API key for authenticating with Azure Document Intelligence.
    /// When null, <see cref="Azure.Identity.DefaultAzureCredential"/> is used instead.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// The model ID to use for document analysis. Defaults to "prebuilt-layout".
    /// </summary>
    public string ModelId { get; set; } = "prebuilt-layout";

    /// <summary>
    /// File extensions that this converter can handle.
    /// </summary>
    public IReadOnlyList<string> SupportedExtensions { get; set; } =
        [".pdf", ".png", ".jpg", ".jpeg", ".tiff", ".bmp", ".docx", ".xlsx", ".pptx"];
}
