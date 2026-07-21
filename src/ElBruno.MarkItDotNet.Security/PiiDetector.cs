using System.Text.RegularExpressions;

namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// An <see cref="ISecurityPolicy"/> that detects and optionally redacts
/// Personally Identifiable Information (PII) patterns such as email addresses,
/// phone numbers, SSNs, credit cards, and API keys.
/// </summary>
public sealed partial class PiiDetector : ISecurityPolicy
{
    private readonly PiiDetectorOptions _options;
    private readonly IReadOnlyList<(Regex Pattern, string RuleName, string Description)> _patterns;

    /// <inheritdoc/>
    public string PolicyName => "PiiDetector";

    /// <summary>Initialises a new <see cref="PiiDetector"/> with default options.</summary>
    public PiiDetector() : this(new PiiDetectorOptions()) { }

    /// <summary>Initialises a new <see cref="PiiDetector"/> with custom options.</summary>
    public PiiDetector(PiiDetectorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
        _patterns = BuildPatterns(options);
    }

    /// <inheritdoc/>
    public Task<PolicyResult> EvaluateAsync(string content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var violations = new List<PolicyViolation>();
        var processed = content;
        var lineOffsets = BuildLineOffsets(content);

        foreach (var (pattern, ruleName, description) in _patterns)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var matches = pattern.Matches(processed);
            if (matches.Count == 0) continue;

            // Record one violation per pattern type (with the first match's line)
            var firstMatchLine = OffsetToLine(matches[0].Index, lineOffsets);
            violations.Add(new PolicyViolation(
                ruleName,
                $"{description} — {matches.Count} occurrence(s) found.",
                firstMatchLine,
                _options.EnableRedaction
                    ? $"PII has been replaced with '{_options.RedactionMask}'."
                    : "Consider redacting this content before sharing."));

            if (_options.EnableRedaction)
                processed = pattern.Replace(processed, _options.RedactionMask);
        }

        var result = violations.Count == 0
            ? PolicyResult.Pass()
            : PolicyResult.Fail(
                violations,
                redactedContent: _options.EnableRedaction ? processed : null,
                metadata: new Dictionary<string, object>
                {
                    ["redactionEnabled"] = _options.EnableRedaction,
                    ["redactionMask"] = _options.RedactionMask,
                    ["violationCount"] = violations.Count
                });

        return Task.FromResult(result);
    }

    // --- helpers ---

    private static List<(Regex, string, string)> BuildPatterns(PiiDetectorOptions opts)
    {
        var list = new List<(Regex, string, string)>();

        if (opts.DetectEmails)
            list.Add((EmailRegex(), "PII_EMAIL", "Email address detected"));

        if (opts.DetectPhones)
            list.Add((PhoneRegex(), "PII_PHONE", "Phone number detected"));

        if (opts.DetectSsn)
            list.Add((SsnRegex(), "PII_SSN", "Social Security Number detected"));

        if (opts.DetectCreditCards)
            list.Add((CreditCardRegex(), "PII_CREDIT_CARD", "Credit card number detected"));

        if (opts.DetectApiKeys)
            list.Add((ApiKeyRegex(), "PII_API_KEY", "API key or secret token detected"));

        foreach (var raw in opts.CustomPatterns)
        {
            if (!string.IsNullOrWhiteSpace(raw))
                list.Add((new Regex(raw, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
                    "PII_CUSTOM", $"Custom PII pattern matched: {raw}"));
        }

        return list;
    }

    private static int[] BuildLineOffsets(string content)
    {
        var offsets = new List<int> { 0 };
        for (var i = 0; i < content.Length; i++)
            if (content[i] == '\n')
                offsets.Add(i + 1);
        return [.. offsets];
    }

    private static int OffsetToLine(int offset, int[] lineOffsets)
    {
        var line = Array.BinarySearch(lineOffsets, offset);
        return (line < 0 ? ~line : line + 1);
    }

    // --- generated regexes ---

    [GeneratedRegex(
        @"\b[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[A-Za-z]{2,}\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(
        @"\b(?:\+?\d{1,2}[\s\-]?)?(?:\(?\d{3}\)?[\s\-]?)\d{3}[\s\-]?\d{4}\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(
        @"\b\d{3}\-\d{2}\-\d{4}\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex SsnRegex();

    [GeneratedRegex(
        @"\b(?:4\d{3}|5[1-5]\d{2}|6011|3[47]\d{2})[\s\-]?\d{4}[\s\-]?\d{4}[\s\-]?\d{4}\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex CreditCardRegex();

    [GeneratedRegex(
        @"\b(?:api[_\-]?key|api[_\-]?secret|access[_\-]?token|auth[_\-]?token|bearer|secret[_\-]?key)\s*[=:]\s*[^\s]{8,}\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ApiKeyRegex();
}
