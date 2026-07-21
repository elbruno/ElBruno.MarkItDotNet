# Security Policies Guide

This guide covers both the **library policy API** in `ElBruno.MarkItDotNet.Security` and the `SecurityPoliciesDemo` sample.

## Library: ISecurityPolicy API (v0.8.0+)

The library now ships a composable policy system alongside the existing `ISecurityScanner`.

### Core types

| Type | Description |
|---|---|
| `ISecurityPolicy` | Interface all policies implement; returns a `PolicyResult` |
| `PolicyResult` | Passed/failed flag + violations list + optional redacted content |
| `PolicyViolation` | Rule name, message, line number, suggested action |
| `PiiDetector` | Detects emails, phones, SSNs, credit cards, API keys |
| `GuardrailsPolicy` | Enforces max content length and max line count |

### Quick start

```csharp
// Run PII detection
var detector = new PiiDetector();
var result = await detector.EvaluateAsync(markdownContent);

if (!result.Passed)
{
    foreach (var v in result.Violations)
        Console.WriteLine($"[{v.RuleName}] line {v.LineNumber}: {v.Message}");

    Console.WriteLine(result.RedactedContent); // PII replaced with [REDACTED]
}
```

### PiiDetector

Detects and optionally redacts:
- **Email addresses** — rule `PII_EMAIL`
- **Phone numbers** — rule `PII_PHONE`
- **Social Security Numbers** — rule `PII_SSN`
- **Credit card numbers** — rule `PII_CREDIT_CARD`
- **API keys / secret tokens** — rule `PII_API_KEY`
- **Custom regex patterns** — rule `PII_CUSTOM`

```csharp
var detector = new PiiDetector(new PiiDetectorOptions
{
    DetectEmails = true,
    DetectCreditCards = true,
    EnableRedaction = true,
    RedactionMask = "***",
    CustomPatterns = [@"\bEMP\d{6}\b"]  // employee ID pattern
});
```

### GuardrailsPolicy

```csharp
var guardrails = new GuardrailsPolicy(new GuardrailsPolicyOptions
{
    MaxContentLength = 500_000,   // characters
    MaxLineCount     = 10_000
});
var result = await guardrails.EvaluateAsync(content);
```

### Dependency injection

```csharp
// Individual registrations
services
    .AddMarkItDotNetSecurity()          // ISecurityScanner
    .AddPiiDetector(opts =>             // ISecurityPolicy: PiiDetector
    {
        opts.EnableRedaction = true;
        opts.RedactionMask = "[REDACTED]";
    })
    .AddContentPolicyEngine(opts =>     // ISecurityPolicy: ContentPolicyEngine
    {
        opts.DenyKeywords.Add("confidential");
        opts.BlockedDomains.Add("malicious.com");
    })
    .AddGuardrailsPolicy(opts =>        // ISecurityPolicy: GuardrailsPolicy
    {
        opts.MaxContentLength = 1_000_000;
    });

// Or use the fluent builder to compose everything in one call
services.AddSecurityPolicies()
        .WithPiiDetector(opts => opts.EnableRedaction = true)
        .WithContentPolicy(opts => opts.DenyKeywords.Add("confidential"))
        .WithGuardrails(opts => opts.MaxContentLength = 500_000)
        .WithAuditLog("logs/security-audit.jsonl")
        .WithChain(shortCircuit: false);  // registers SecurityPolicyChain

// Resolve all policies as IEnumerable<ISecurityPolicy>
var policies = serviceProvider.GetServices<ISecurityPolicy>();

// Or resolve the composed chain directly
var chain = serviceProvider.GetRequiredService<SecurityPolicyChain>();
```

### SecurityPolicyChain

Chains multiple policies together. Each policy sees the (potentially already-redacted) output of the previous one.

```csharp
var chain = new SecurityPolicyChain(
    shortCircuit: false,             // false = collect all violations; true = stop at first failure
    new PiiDetector(),
    new ContentPolicyEngine(new ContentPolicyOptions { DenyKeywords = ["secret"] }),
    new GuardrailsPolicy());

var result = await chain.EvaluateAsync(content);

if (!result.Passed)
{
    foreach (var v in result.Violations)
        Console.WriteLine($"[{v.RuleName}] {v.Message}");
}

// result.RedactedContent has PII already replaced if PiiDetector ran
```

### SecurityAuditLog

Append-only JSONL audit log for compliance tracking.

```csharp
var auditLog = new SecurityAuditLog("logs/security-audit.jsonl");

// After each evaluation:
await auditLog.AppendAsync("document.pdf", chain.PolicyName, result);

// Read back all entries:
var entries = await auditLog.ReadAllAsync();
foreach (var entry in entries)
    Console.WriteLine($"{entry.Timestamp:s} [{entry.Policy}] {entry.Source} — passed={entry.Passed}");
```

---

## Sample: SecurityPoliciesDemo

The sample composes scanner rules + the policy engine into a single pipeline per document.

### What the sample applies

1. **Scanner rules** from `ElBruno.MarkItDotNet.Security`:
   - JavaScript links
   - Secret-like token detection
   - Control character detection
2. **Custom policy rules**:
   - Deny keyword checks (for example `confidential`)
   - Max content length guardrail
   - PII pattern detection (email, phone, SSN) with optional redaction
3. **Audit output**:
   - JSONL entry per scenario with scanner and policy outcomes

### Run

```bash
dotnet run --project src/samples/SecurityPoliciesDemo/SecurityPoliciesDemo.csproj
```

Dry-run:

```bash
dotnet run --project src/samples/SecurityPoliciesDemo/SecurityPoliciesDemo.csproj -- --dry-run
```

### Configuration

`src/samples/SecurityPoliciesDemo/appsettings.json` (`SecurityPoliciesDemo` section):

- `OutputPath`
- `AuditLogPath`
- `MaxIssues`
- `MaxContentLength`
- `EnablePiiRedaction`
- `RedactionMask`
- `DenyKeywords`
- scanner toggles (`DetectJavaScriptLinks`, `DetectSecretLikeTokens`, `DetectControlCharacters`)

CLI arguments override these values:

```bash
dotnet run --project src/samples/SecurityPoliciesDemo/SecurityPoliciesDemo.csproj -- --output ./security-output --audit-log ./security-output/audit.jsonl --deny-keyword internal-only
```

## Integration notes

- Use `--dry-run` in CI or experimentation flows when you only need policy results.
- Keep deny keywords domain-specific (legal, compliance, tenant terms).
- Treat PII regex matching as baseline heuristics; production apps can plug in stronger detectors upstream.

---

## MarkdownService integration (v0.9.0+)

`ElBruno.MarkItDotNet.Security` integrates directly with `MarkdownService` and the connector pipeline through two new extension points.

### ConvertWithPolicyAsync

Run a policy check as part of the conversion step. The result wraps both the `ConversionResult` and the `PolicyResult`.

```csharp
using ElBruno.MarkItDotNet.Security;

var service = serviceProvider.GetRequiredService<MarkdownService>();
var policy  = new PiiDetector(new PiiDetectorOptions { EnableRedaction = true });

// From a file path
PolicyConversionResult result = await service.ConvertWithPolicyAsync("report.pdf", policy);

// From a stream
await using var stream = File.OpenRead("report.pdf");
PolicyConversionResult result = await service.ConvertWithPolicyAsync(stream, ".pdf", policy);

// Inspect
if (!result.IsClean)
{
    foreach (var v in result.Policy.Violations)
        Console.WriteLine($"[{v.RuleName}] line {v.LineNumber}: {v.Message}");
}

// EffectiveMarkdown returns redacted content when available, otherwise raw converted markdown
string markdown = result.EffectiveMarkdown!;
```

**`PolicyConversionResult` properties:**

| Property | Type | Description |
|---|---|---|
| `Conversion` | `ConversionResult` | Full conversion result from `MarkdownService` |
| `Policy` | `PolicyResult` | Full policy evaluation result |
| `IsClean` | `bool` | `true` only when both conversion succeeded and policy passed |
| `EffectiveMarkdown` | `string?` | Redacted content if available, otherwise raw markdown; `null` on conversion failure |

> When conversion fails (e.g. unsupported file format), `Policy` is `PolicyResult.Pass()` and `IsClean` is `false` because `Conversion.Success` is `false`.

### PolicyFilteredDocumentSource

Wrap any `IDocumentSource` (e.g. `FileSystemConnector`) to gate documents before they enter the pipeline. Filters can apply independently or together:

1. **Metadata predicate** — fast pre-filter before opening any streams
2. **Content policy gate** — converts each candidate document and runs a policy; excluded if policy fails

```csharp
var connector = serviceProvider.GetRequiredService<FileSystemConnector>();
var service   = serviceProvider.GetRequiredService<MarkdownService>();
var policy    = new SecurityPolicyChain(false, new PiiDetector(), new ContentPolicyEngine(...));

// Metadata only — no conversion cost
var filtered = new PolicyFilteredDocumentSource(
    connector,
    doc => doc.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase));

// Content policy only
var filtered = new PolicyFilteredDocumentSource(connector, service, policy);

// Both — metadata runs first (cheap), then content policy
var filtered = new PolicyFilteredDocumentSource(
    connector,
    service,
    policy,
    doc => doc.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase));

// Use like any IDocumentSource
await foreach (var doc in filtered)
    Console.WriteLine(doc.Name);

// CountAsync and ValidateAsync also respect the filter
int count  = await filtered.CountAsync();
bool valid = await filtered.ValidateAsync();
```

**Behaviour notes:**
- Documents with unrecognised file extensions skip the content policy and pass through (so non-markdown files aren't blocked by conversion errors).
- The metadata predicate is evaluated before any stream is opened (zero I/O cost).
- `CountAsync` materialises the async enumerable to count only passing documents.

---

## What remains as future enhancements

- `ContentPolicyOptions.ConditionalRules` — rule DSL (`IF source='x' THEN block`)
- `AllowedDomains` positive allowlist (block everything except listed domains)
- Database-backed audit storage (currently file-only JSONL)
