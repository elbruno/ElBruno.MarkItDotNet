using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.Identity;
using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.DocumentIntelligence;

/// <summary>
/// Converts documents using Azure Document Intelligence with layout-aware OCR extraction.
/// Implements <see cref="IStructuredConverter"/> to integrate with the CoreModel pipeline.
/// </summary>
public class DocumentIntelligenceConverter : IStructuredConverter
{
    private readonly DocumentIntelligenceOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentIntelligenceConverter"/> class.
    /// </summary>
    /// <param name="options">Configuration options for Azure Document Intelligence.</param>
    public DocumentIntelligenceConverter(DocumentIntelligenceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <summary>
    /// Determines whether this converter can handle the given file.
    /// </summary>
    /// <param name="filePath">The path to the file to check.</param>
    /// <returns><see langword="true"/> if the file extension is supported; otherwise, <see langword="false"/>.</returns>
    public bool CanHandle(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return _options.SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Converts a file at the given path to a structured <see cref="Document"/> using Azure Document Intelligence.
    /// </summary>
    /// <param name="filePath">The path to the file to convert.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A structured <see cref="Document"/> representing the analyzed content.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the endpoint is not configured.</exception>
    public async Task<Document> ConvertToDocumentAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        using var stream = File.OpenRead(filePath);
        return await ConvertToDocumentAsync(stream, filePath, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Converts a stream to a structured <see cref="Document"/> using Azure Document Intelligence.
    /// </summary>
    /// <param name="stream">The input stream containing file content.</param>
    /// <param name="fileName">The original file name (used for extension detection and source reference).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A structured <see cref="Document"/> representing the analyzed content.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the endpoint is not configured.</exception>
    public async Task<Document> ConvertToDocumentAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var client = CreateClient();
        var result = await AnalyzeAsync(client, stream, cancellationToken).ConfigureAwait(false);

        return DocumentIntelligenceMapper.MapToDocument(result, fileName);
    }

    private DocumentIntelligenceClient CreateClient()
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint))
        {
            throw new InvalidOperationException(
                "Azure Document Intelligence endpoint is not configured. " +
                "Set the Endpoint property in DocumentIntelligenceOptions.");
        }

        var endpoint = new Uri(_options.Endpoint);

        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new DocumentIntelligenceClient(endpoint, new AzureKeyCredential(_options.ApiKey));
        }

        return new DocumentIntelligenceClient(endpoint, new DefaultAzureCredential());
    }

    private async Task<AnalyzeResult> AnalyzeAsync(
        DocumentIntelligenceClient client,
        Stream stream,
        CancellationToken cancellationToken)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
            memoryStream.Position = 0;

            var analyzeOptions = new AnalyzeDocumentOptions(_options.ModelId, BinaryData.FromBytes(memoryStream.ToArray()));

            var operation = await client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                analyzeOptions,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return operation.Value;
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException(
                $"Azure Document Intelligence analysis failed: {ex.Message} (Status: {ex.Status})", ex);
        }
    }
}
