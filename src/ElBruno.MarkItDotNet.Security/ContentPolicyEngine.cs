using System.Text.RegularExpressions;

namespace ElBruno.MarkItDotNet.Security;

/// <summary>
/// An <see cref="ISecurityPolicy"/> that evaluates content against configurable
/// allow/deny keyword lists and blocked domain rules.
/// </summary>
public sealed partial class ContentPolicyEngine : ISecurityPolicy
{
    private readonly ContentPolicyOptions _options;

    /// <inheritdoc/>
    public string PolicyName => "ContentPolicyEngine";

    /// <summary>Initialises a new <see cref="ContentPolicyEngine"/> with default options.</summary>
    public ContentPolicyEngine() : this(new ContentPolicyOptions()) { }

    /// <summary>Initialises a new <see cref="ContentPolicyEngine"/> with custom options.</summary>
    public ContentPolicyEngine(ContentPolicyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <inheritdoc/>
    public Task<PolicyResult> EvaluateAsync(string content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var violations = new List<PolicyViolation>();
        var lineOffsets = BuildLineOffsets(content);

        // --- deny keywords ---
        foreach (var keyword in _options.DenyKeywords.Where(k => !string.IsNullOrWhiteSpace(k)))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var idx = content.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                violations.Add(new PolicyViolation(
                    "DENY_KEYWORD",
                    $"Denied keyword '{keyword}' found in content.",
                    OffsetToLine(idx, lineOffsets),
                    "Remove or replace the denied keyword before processing."));

                if (_options.ShortCircuit) break;
            }
        }

        if (!_options.ShortCircuit || violations.Count == 0)
        {
            // --- blocked domains ---
            foreach (var domain in _options.BlockedDomains.Where(d => !string.IsNullOrWhiteSpace(d)))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var matches = DomainUrlRegex().Matches(content);
                foreach (Match m in matches)
                {
                    if (m.Value.Contains(domain, StringComparison.OrdinalIgnoreCase))
                    {
                        violations.Add(new PolicyViolation(
                            "BLOCKED_DOMAIN",
                            $"Reference to blocked domain '{domain}' found: {m.Value}",
                            OffsetToLine(m.Index, lineOffsets),
                            $"Remove or replace the link referencing '{domain}'."));

                        if (_options.ShortCircuit) break;
                    }
                }

                if (_options.ShortCircuit && violations.Count > 0) break;
            }
        }

        var result = violations.Count == 0
            ? PolicyResult.Pass()
            : PolicyResult.Fail(violations);

        return Task.FromResult(result);
    }

    // --- helpers ---

    private static int[] BuildLineOffsets(string content)
    {
        var offsets = new List<int> { 0 };
        for (var i = 0; i < content.Length; i++)
            if (content[i] == '\n') offsets.Add(i + 1);
        return [.. offsets];
    }

    private static int OffsetToLine(int offset, int[] lineOffsets)
    {
        var line = Array.BinarySearch(lineOffsets, offset);
        return line < 0 ? ~line : line + 1;
    }

    [GeneratedRegex(@"https?://[^\s\)\]""']+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DomainUrlRegex();
}
