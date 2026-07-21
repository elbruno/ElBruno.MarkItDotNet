using System.Text;
using System.Text.Json;
using ElBruno.MarkItDotNet;
using ElBruno.MarkItDotNet.Security;
using Microsoft.Extensions.DependencyInjection;
using SecurityPoliciesDemoSample;

SecurityDemoOptions options;
try
{
    options = SecurityDemoOptionsParser.Parse(args, AppContext.BaseDirectory);
}
catch (ArgumentException ex)
{
    Console.WriteLine(ex.Message);
    Console.WriteLine();
    PrintUsage();
    return;
}

if (options.ShowHelp)
{
    PrintUsage();
    return;
}

Console.WriteLine("=== Phase 3: Security Policies Demo ===");
Console.WriteLine($"Mode             : {(options.DryRun ? "DRY-RUN" : "WRITE OUTPUTS")}");
Console.WriteLine($"Output path      : {options.OutputPath}");
Console.WriteLine($"Audit log path   : {options.AuditLogPath}");
Console.WriteLine($"Max content chars: {options.MaxContentLength}");
Console.WriteLine($"PII redaction    : {(options.EnablePiiRedaction ? "ENABLED" : "DISABLED")}");
Console.WriteLine();

if (!options.DryRun)
{
    Directory.CreateDirectory(options.OutputPath);
    Directory.CreateDirectory(Path.GetDirectoryName(options.AuditLogPath)!);
}

var services = new ServiceCollection();
services.AddMarkItDotNet();
services.AddMarkItDotNetSecurity(scannerOptions =>
{
    scannerOptions.MaxIssues = options.MaxIssues;
    scannerOptions.DetectJavaScriptLinks = options.DetectJavaScriptLinks;
    scannerOptions.DetectSecretLikeTokens = options.DetectSecretLikeTokens;
    scannerOptions.DetectControlCharacters = options.DetectControlCharacters;
});
using var provider = services.BuildServiceProvider();

var markdownService = provider.GetRequiredService<MarkdownService>();
var scanner = provider.GetRequiredService<ISecurityScanner>();

var policyRules = new SecurityPolicyRules
{
    DenyKeywords = options.DenyKeywords,
    MaxContentLength = options.MaxContentLength,
    EnablePiiRedaction = options.EnablePiiRedaction,
    RedactionMask = options.RedactionMask
};

var scenarios = BuildScenarios();
Console.WriteLine($"Running {scenarios.Count} security scenario(s).");
Console.WriteLine();

var safeCount = 0;
var unsafeCount = 0;
var auditEntries = new List<AuditEntry>();

foreach (var scenario in scenarios)
{
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(scenario.SourceText));
    var conversion = await markdownService.ConvertAsync(stream, scenario.Extension);
    if (!conversion.Success)
    {
        Console.WriteLine($"[FAIL] {scenario.Name}: conversion failed - {conversion.ErrorMessage}");
        unsafeCount++;
        Console.WriteLine();
        continue;
    }

    var scan = scanner.Scan(conversion.Markdown);
    var policy = SecurityPolicyEngine.Evaluate(conversion.Markdown, policyRules);
    var isSafe = scan.IsSafe && policy.Passed;

    if (isSafe)
    {
        safeCount++;
    }
    else
    {
        unsafeCount++;
    }

    var outputFile = $"{ToSafeFileName(scenario.Name)}.md";
    var outputPath = Path.Combine(options.OutputPath, outputFile);
    if (!options.DryRun)
    {
        await File.WriteAllTextAsync(outputPath, policy.ProcessedContent, Encoding.UTF8);
    }

    Console.WriteLine($"[{(isSafe ? "SAFE" : "UNSAFE")}] {scenario.Name} ({scenario.Extension})");
    Console.WriteLine($"  Output       : {(options.DryRun ? $"[DRY-RUN] {outputPath}" : outputPath)}");
    Console.WriteLine($"  Scanner score: {scan.Score:F2} (Issues: {scan.Issues.Count})");
    Console.WriteLine($"  Policy issues: {policy.Violations.Count}");

    foreach (var issue in scan.Issues)
    {
        var offset = issue.Offset.HasValue ? $" @ {issue.Offset}" : string.Empty;
        Console.WriteLine($"  - scanner/{issue.Severity}: {issue.Code}{offset} - {issue.Message}");
    }

    foreach (var violation in policy.Violations)
    {
        Console.WriteLine($"  - policy/{violation.Severity}: {violation.Code} - {violation.Message}");
    }

    if (scan.Issues.Count == 0 && policy.Violations.Count == 0)
    {
        Console.WriteLine("  - none");
    }

    auditEntries.Add(
        new AuditEntry(
            scenario.Name,
            scenario.Extension,
            isSafe,
            scan.Score,
            scan.Issues.Count,
            policy.Violations.Select(v => v.Code).ToArray()));

    Console.WriteLine();
}

if (!options.DryRun)
{
    await WriteAuditLogAsync(options.AuditLogPath, auditEntries);
}

Console.WriteLine($"Security scan complete. Safe: {safeCount} | Unsafe: {unsafeCount}");
Console.WriteLine($"Audit log: {(options.DryRun ? $"[DRY-RUN] {options.AuditLogPath}" : options.AuditLogPath)}");

static async Task WriteAuditLogAsync(string path, IReadOnlyCollection<AuditEntry> entries)
{
    await using var stream = File.Create(path);
    await using var writer = new StreamWriter(stream, Encoding.UTF8);
    foreach (var entry in entries)
    {
        await writer.WriteLineAsync(JsonSerializer.Serialize(entry));
    }
}

static string ToSafeFileName(string scenarioName)
{
    var invalid = Path.GetInvalidFileNameChars();
    var chars = scenarioName
        .ToLowerInvariant()
        .Select(ch => invalid.Contains(ch) || char.IsWhiteSpace(ch) ? '-' : ch)
        .ToArray();
    return string.Join(string.Empty, chars).Trim('-');
}

static List<SecurityScenario> BuildScenarios()
{
    return
    [
        new SecurityScenario(
            Name: "Clean Markdown Text",
            Extension: ".txt",
            SourceText:
            """
            # Weekly Summary

            All systems are healthy.
            Conversion and evaluation workflows are passing.
            """
        ),
        new SecurityScenario(
            Name: "PII and Secret Token",
            Extension: ".txt",
            SourceText:
            """
            Contact: jane.doe@contoso.com
            Phone: 555-123-4567
            SSN: 123-45-6789
            Deployment note: token_sk_AbcDefGhijk1234567890 must be rotated.
            """
        ),
        new SecurityScenario(
            Name: "JavaScript Link and Denied Keyword",
            Extension: ".html",
            SourceText:
            """
            <html><body>
              <h1>Confidential Release Notes</h1>
              <a href="javascript:alert('xss')">click me</a>
            </body></html>
            """
        )
    ];
}

static void PrintUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project src/samples/SecurityPoliciesDemo/SecurityPoliciesDemo.csproj [options]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --output <path>               Output folder for processed markdown");
    Console.WriteLine("  --audit-log <path>            JSONL audit output path");
    Console.WriteLine("  --max-content-length <chars>  Guardrail max content length");
    Console.WriteLine("  --deny-keyword <word>         Extra denied keyword (repeatable)");
    Console.WriteLine("  --redaction-mask <text>       Redaction token");
    Console.WriteLine("  --disable-pii-redaction       Detect but do not redact PII patterns");
    Console.WriteLine("  --dry-run                     Process without writing output files");
    Console.WriteLine("  --help                        Show this help");
}

internal sealed record SecurityScenario(string Name, string Extension, string SourceText);
internal sealed record AuditEntry(
    string Scenario,
    string Extension,
    bool IsSafe,
    double ScannerScore,
    int ScannerIssueCount,
    IReadOnlyCollection<string> PolicyCodes);
