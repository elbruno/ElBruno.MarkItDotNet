namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// Extension methods that add security policy evaluation to <see cref="MarkdownService"/>.
/// </summary>
public static class MarkdownServiceSecurityExtensions
{
    /// <summary>
    /// Converts the file at <paramref name="filePath"/> to Markdown, then evaluates the
    /// converted content against <paramref name="policy"/>.
    /// </summary>
    /// <param name="service">The markdown service used for conversion.</param>
    /// <param name="filePath">Path to the file to convert.</param>
    /// <param name="policy">The security policy to evaluate against the converted Markdown.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A <see cref="PolicyConversionResult"/> with both conversion and policy outcomes.
    /// <see cref="PolicyConversionResult.EffectiveMarkdown"/> contains the redacted Markdown
    /// when the policy produced a redaction, otherwise the original.
    /// </returns>
    public static async Task<PolicyConversionResult> ConvertWithPolicyAsync(
        this MarkdownService service,
        string filePath,
        ISecurityPolicy policy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(policy);

        var conversion = await service.ConvertAsync(filePath, cancellationToken).ConfigureAwait(false);

        if (!conversion.Success)
        {
            // Skip policy evaluation on failed conversion
            return new PolicyConversionResult(conversion, PolicyResult.Pass());
        }

        var policyResult = await policy.EvaluateAsync(conversion.Markdown, cancellationToken).ConfigureAwait(false);
        return new PolicyConversionResult(conversion, policyResult);
    }

    /// <summary>
    /// Converts a stream to Markdown, then evaluates the converted content against <paramref name="policy"/>.
    /// </summary>
    /// <param name="service">The markdown service used for conversion.</param>
    /// <param name="stream">The input stream containing file content.</param>
    /// <param name="fileExtension">File extension including the leading dot (e.g., ".txt").</param>
    /// <param name="policy">The security policy to evaluate against the converted Markdown.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task<PolicyConversionResult> ConvertWithPolicyAsync(
        this MarkdownService service,
        Stream stream,
        string fileExtension,
        ISecurityPolicy policy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(policy);

        var conversion = await service.ConvertAsync(stream, fileExtension, cancellationToken).ConfigureAwait(false);

        if (!conversion.Success)
            return new PolicyConversionResult(conversion, PolicyResult.Pass());

        var policyResult = await policy.EvaluateAsync(conversion.Markdown, cancellationToken).ConfigureAwait(false);
        return new PolicyConversionResult(conversion, policyResult);
    }
}
