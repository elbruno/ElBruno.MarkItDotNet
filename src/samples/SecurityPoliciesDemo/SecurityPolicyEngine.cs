using System.Text.RegularExpressions;

namespace SecurityPoliciesDemoSample;

public sealed record PolicyViolation(string Code, string Severity, string Message);

public sealed record PolicyEvaluationResult(bool Passed, string ProcessedContent, IReadOnlyList<PolicyViolation> Violations);

public sealed class SecurityPolicyRules
{
    public required IReadOnlyCollection<string> DenyKeywords { get; init; }
    public required int MaxContentLength { get; init; }
    public required bool EnablePiiRedaction { get; init; }
    public required string RedactionMask { get; init; }
}

public static class SecurityPolicyEngine
{
    private static readonly Regex EmailRegex = new(
        @"\b[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[A-Za-z]{2,}\b",
        RegexOptions.Compiled);

    private static readonly Regex PhoneRegex = new(
        @"\b(?:\+?\d{1,2}[\s\-]?)?(?:\(?\d{3}\)?[\s\-]?)\d{3}[\s\-]?\d{4}\b",
        RegexOptions.Compiled);

    private static readonly Regex SsnRegex = new(
        @"\b\d{3}\-\d{2}\-\d{4}\b",
        RegexOptions.Compiled);

    public static PolicyEvaluationResult Evaluate(string markdown, SecurityPolicyRules rules)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        ArgumentNullException.ThrowIfNull(rules);

        var violations = new List<PolicyViolation>();
        var processedContent = markdown;

        if (markdown.Length > rules.MaxContentLength)
        {
            violations.Add(
                new PolicyViolation(
                    "GUARDRAIL_CONTENT_LENGTH",
                    "High",
                    $"Content length {markdown.Length} exceeds max allowed {rules.MaxContentLength}."));
        }

        foreach (var keyword in rules.DenyKeywords.Where(k => !string.IsNullOrWhiteSpace(k)))
        {
            if (processedContent.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                violations.Add(
                    new PolicyViolation(
                        "DENY_KEYWORD",
                        "Medium",
                        $"Denied keyword detected: '{keyword}'."));
            }
        }

        var piiDetected = false;
        piiDetected |= EmailRegex.IsMatch(processedContent);
        piiDetected |= PhoneRegex.IsMatch(processedContent);
        piiDetected |= SsnRegex.IsMatch(processedContent);

        if (piiDetected)
        {
            violations.Add(
                new PolicyViolation(
                    "PII_DETECTED",
                    "High",
                    "Potential PII detected (email, phone, or SSN pattern)."));

            if (rules.EnablePiiRedaction)
            {
                processedContent = EmailRegex.Replace(processedContent, rules.RedactionMask);
                processedContent = PhoneRegex.Replace(processedContent, rules.RedactionMask);
                processedContent = SsnRegex.Replace(processedContent, rules.RedactionMask);
            }
        }

        return new PolicyEvaluationResult(
            Passed: violations.Count == 0,
            ProcessedContent: processedContent,
            Violations: violations);
    }
}
