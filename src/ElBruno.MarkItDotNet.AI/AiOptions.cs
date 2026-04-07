namespace ElBruno.MarkItDotNet.AI;

/// <summary>
/// Options for AI-powered converters.
/// </summary>
public class AiOptions
{
    /// <summary>
    /// Prompt sent to the AI model when processing images.
    /// <para>
    /// ⚠️ Security: This prompt is sent alongside untrusted image content.
    /// Ensure the prompt does not encourage the model to follow instructions found within images.
    /// </para>
    /// </summary>
    public string? ImagePrompt { get; set; } = "Describe this image in detail. Extract any visible text using OCR. Return the result as Markdown.";

    /// <summary>
    /// Prompt sent to the AI model when processing audio.
    /// <para>
    /// ⚠️ Security: This prompt is sent alongside untrusted audio content.
    /// Ensure the prompt does not encourage the model to follow instructions found within audio.
    /// </para>
    /// </summary>
    public string? AudioPrompt { get; set; } = "Transcribe this audio content. Return the result as Markdown.";
}
