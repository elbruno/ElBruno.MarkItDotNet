using System.Globalization;
using System.Text.Json;

namespace SecurityPoliciesDemoSample;

public sealed class SecurityDemoOptions
{
    public string OutputPath { get; set; } = "output";
    public string AuditLogPath { get; set; } = "output\\security-audit.jsonl";
    public bool DryRun { get; set; }
    public int MaxIssues { get; set; } = 20;
    public bool DetectJavaScriptLinks { get; set; } = true;
    public bool DetectSecretLikeTokens { get; set; } = true;
    public bool DetectControlCharacters { get; set; } = true;
    public int MaxContentLength { get; set; } = 20_000;
    public bool EnablePiiRedaction { get; set; } = true;
    public string RedactionMask { get; set; } = "[REDACTED]";
    public List<string> DenyKeywords { get; set; } = ["confidential", "secret"];
    public bool ShowHelp { get; set; }
}

public static class SecurityDemoOptionsParser
{
    public static SecurityDemoOptions Parse(string[] args, string baseDirectory)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(baseDirectory);

        var options = LoadDefaults(baseDirectory);

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (string.Equals(arg, "--help", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(arg, "-h", StringComparison.OrdinalIgnoreCase))
            {
                options.ShowHelp = true;
                continue;
            }

            if (string.Equals(arg, "--dry-run", StringComparison.OrdinalIgnoreCase))
            {
                options.DryRun = true;
                continue;
            }

            if (string.Equals(arg, "--disable-pii-redaction", StringComparison.OrdinalIgnoreCase))
            {
                options.EnablePiiRedaction = false;
                continue;
            }

            if (TryReadValue(arg, "--output", out var outputPath))
            {
                options.OutputPath = outputPath ?? ReadNext(args, ref i, "--output");
                continue;
            }

            if (TryReadValue(arg, "--audit-log", out var auditLogPath))
            {
                options.AuditLogPath = auditLogPath ?? ReadNext(args, ref i, "--audit-log");
                continue;
            }

            if (TryReadValue(arg, "--max-content-length", out var maxLengthRaw))
            {
                options.MaxContentLength = int.Parse(maxLengthRaw ?? ReadNext(args, ref i, "--max-content-length"), CultureInfo.InvariantCulture);
                continue;
            }

            if (TryReadValue(arg, "--deny-keyword", out var keywordRaw))
            {
                options.DenyKeywords.Add(keywordRaw ?? ReadNext(args, ref i, "--deny-keyword"));
                continue;
            }

            if (TryReadValue(arg, "--redaction-mask", out var maskRaw))
            {
                options.RedactionMask = maskRaw ?? ReadNext(args, ref i, "--redaction-mask");
                continue;
            }

            throw new ArgumentException($"Unknown argument: '{arg}'.");
        }

        if (options.MaxIssues <= 0)
        {
            throw new ArgumentException("MaxIssues must be greater than 0.");
        }

        if (options.MaxContentLength <= 0)
        {
            throw new ArgumentException("--max-content-length must be greater than 0.");
        }

        if (string.IsNullOrWhiteSpace(options.RedactionMask))
        {
            throw new ArgumentException("RedactionMask cannot be empty.");
        }

        options.OutputPath = Path.GetFullPath(options.OutputPath, baseDirectory);
        options.AuditLogPath = Path.GetFullPath(options.AuditLogPath, baseDirectory);

        return options;
    }

    private static SecurityDemoOptions LoadDefaults(string baseDirectory)
    {
        var options = new SecurityDemoOptions();
        var appSettingsPath = Path.Combine(baseDirectory, "appsettings.json");
        if (!File.Exists(appSettingsPath))
        {
            return options;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(appSettingsPath));
        if (!document.RootElement.TryGetProperty("SecurityPoliciesDemo", out var root))
        {
            return options;
        }

        if (TryGetString(root, "OutputPath", out var outputPath))
        {
            options.OutputPath = outputPath!;
        }

        if (TryGetString(root, "AuditLogPath", out var auditPath))
        {
            options.AuditLogPath = auditPath!;
        }

        if (TryGetBool(root, "DryRun", out var dryRun))
        {
            options.DryRun = dryRun;
        }

        if (TryGetInt(root, "MaxIssues", out var maxIssues))
        {
            options.MaxIssues = maxIssues;
        }

        if (TryGetBool(root, "DetectJavaScriptLinks", out var detectJs))
        {
            options.DetectJavaScriptLinks = detectJs;
        }

        if (TryGetBool(root, "DetectSecretLikeTokens", out var detectSecrets))
        {
            options.DetectSecretLikeTokens = detectSecrets;
        }

        if (TryGetBool(root, "DetectControlCharacters", out var detectControls))
        {
            options.DetectControlCharacters = detectControls;
        }

        if (TryGetInt(root, "MaxContentLength", out var maxContentLength))
        {
            options.MaxContentLength = maxContentLength;
        }

        if (TryGetBool(root, "EnablePiiRedaction", out var enablePii))
        {
            options.EnablePiiRedaction = enablePii;
        }

        if (TryGetString(root, "RedactionMask", out var mask))
        {
            options.RedactionMask = mask!;
        }

        if (root.TryGetProperty("DenyKeywords", out var keywords) && keywords.ValueKind == JsonValueKind.Array)
        {
            var configured = keywords.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToList();

            if (configured.Count > 0)
            {
                options.DenyKeywords = configured!;
            }
        }

        return options;
    }

    private static bool TryReadValue(string arg, string optionName, out string? value)
    {
        var prefix = $"{optionName}=";
        if (arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            value = arg[prefix.Length..];
            return true;
        }

        if (string.Equals(arg, optionName, StringComparison.OrdinalIgnoreCase))
        {
            value = null;
            return true;
        }

        value = null;
        return false;
    }

    private static string ReadNext(string[] args, ref int index, string optionName)
    {
        if (index + 1 >= args.Length || args[index + 1].StartsWith("-", StringComparison.Ordinal))
        {
            throw new ArgumentException($"Missing value for {optionName}.");
        }

        index++;
        return args[index];
    }

    private static bool TryGetString(JsonElement element, string name, out string? value)
    {
        if (element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String)
        {
            value = property.GetString();
            return true;
        }

        value = null;
        return false;
    }

    private static bool TryGetBool(JsonElement element, string name, out bool value)
    {
        if (element.TryGetProperty(name, out var property) &&
            (property.ValueKind == JsonValueKind.True || property.ValueKind == JsonValueKind.False))
        {
            value = property.GetBoolean();
            return true;
        }

        value = default;
        return false;
    }

    private static bool TryGetInt(JsonElement element, string name, out int value)
    {
        if (element.TryGetProperty(name, out var property) &&
            property.ValueKind == JsonValueKind.Number &&
            property.TryGetInt32(out value))
        {
            return true;
        }

        value = default;
        return false;
    }
}
