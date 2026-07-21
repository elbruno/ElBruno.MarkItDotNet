namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// Combines a <see cref="ConversionResult"/> with the outcome of a security policy evaluation.
/// </summary>
/// <param name="Conversion">The underlying conversion result.</param>
/// <param name="Policy">The policy evaluation result, evaluated against the converted Markdown.</param>
public sealed record PolicyConversionResult(
    ConversionResult Conversion,
    PolicyResult Policy)
{
    /// <summary>
    /// True when both the conversion succeeded and the policy passed.
    /// </summary>
    public bool IsClean => Conversion.Success && Policy.Passed;

    /// <summary>
    /// The effective Markdown content: the redacted version when the policy produced one,
    /// otherwise the original converted Markdown.
    /// </summary>
    public string EffectiveMarkdown =>
        Policy.RedactedContent ?? Conversion.Markdown;
}
