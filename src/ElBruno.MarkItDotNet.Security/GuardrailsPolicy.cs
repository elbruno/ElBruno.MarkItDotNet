namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// An <see cref="ISecurityPolicy"/> that enforces resource guardrails such as
/// maximum content length and maximum line count.
/// </summary>
public sealed class GuardrailsPolicy : ISecurityPolicy
{
    private readonly GuardrailsPolicyOptions _options;

    /// <inheritdoc/>
    public string PolicyName => "GuardrailsPolicy";

    /// <summary>Initialises a new <see cref="GuardrailsPolicy"/> with default options.</summary>
    public GuardrailsPolicy() : this(new GuardrailsPolicyOptions()) { }

    /// <summary>Initialises a new <see cref="GuardrailsPolicy"/> with custom options.</summary>
    public GuardrailsPolicy(GuardrailsPolicyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <inheritdoc/>
    public Task<PolicyResult> EvaluateAsync(string content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var violations = new List<PolicyViolation>();

        if (content.Length > _options.MaxContentLength)
        {
            violations.Add(new PolicyViolation(
                "GUARDRAIL_CONTENT_LENGTH",
                $"Content length {content.Length:N0} characters exceeds the maximum of {_options.MaxContentLength:N0}.",
                SuggestedAction: "Truncate or split the document before processing."));
        }

        if (_options.MaxLineCount > 0)
        {
            var lineCount = CountLines(content);
            if (lineCount > _options.MaxLineCount)
            {
                violations.Add(new PolicyViolation(
                    "GUARDRAIL_LINE_COUNT",
                    $"Line count {lineCount:N0} exceeds the maximum of {_options.MaxLineCount:N0}.",
                    SuggestedAction: "Split the document into smaller parts."));
            }
        }

        var result = violations.Count == 0
            ? PolicyResult.Pass()
            : PolicyResult.Fail(violations);

        return Task.FromResult(result);
    }

    private static int CountLines(string content)
    {
        if (content.Length == 0) return 0;
        var count = 1;
        foreach (var c in content)
            if (c == '\n') count++;
        return count;
    }
}
