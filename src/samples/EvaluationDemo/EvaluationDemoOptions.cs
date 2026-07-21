using System.Globalization;
using System.Text.Json;

namespace EvaluationDemoSample;

public sealed class EvaluationDemoOptions
{
    public double PassThreshold { get; set; } = 0.70;
    public bool DryRun { get; set; }
    public bool ExportJson { get; set; } = true;
    public bool ExportCsv { get; set; } = true;
    public string OutputPath { get; set; } = "output";
    public string JsonFileName { get; set; } = "evaluation-report.json";
    public string CsvFileName { get; set; } = "evaluation-report.csv";
    public bool ShowHelp { get; set; }
}

public static class EvaluationDemoOptionsParser
{
    public static EvaluationDemoOptions Parse(string[] args, string baseDirectory)
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

            if (string.Equals(arg, "--no-json", StringComparison.OrdinalIgnoreCase))
            {
                options.ExportJson = false;
                continue;
            }

            if (string.Equals(arg, "--no-csv", StringComparison.OrdinalIgnoreCase))
            {
                options.ExportCsv = false;
                continue;
            }

            if (TryReadValue(arg, "--threshold", out var threshold))
            {
                options.PassThreshold = double.Parse(threshold ?? ReadNext(args, ref i, "--threshold"), CultureInfo.InvariantCulture);
                continue;
            }

            if (TryReadValue(arg, "--output", out var output))
            {
                options.OutputPath = output ?? ReadNext(args, ref i, "--output");
                continue;
            }

            throw new ArgumentException($"Unknown argument: '{arg}'.");
        }

        if (options.PassThreshold is < 0 or > 1)
        {
            throw new ArgumentException("--threshold must be between 0.0 and 1.0.");
        }

        options.OutputPath = Path.GetFullPath(options.OutputPath, baseDirectory);
        return options;
    }

    private static EvaluationDemoOptions LoadDefaults(string baseDirectory)
    {
        var options = new EvaluationDemoOptions();
        var appSettingsPath = Path.Combine(baseDirectory, "appsettings.json");
        if (!File.Exists(appSettingsPath))
        {
            return options;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(appSettingsPath));
        if (!document.RootElement.TryGetProperty("EvaluationDemo", out var root))
        {
            return options;
        }

        if (TryGetDouble(root, "PassThreshold", out var passThreshold))
        {
            options.PassThreshold = passThreshold;
        }

        if (TryGetBool(root, "DryRun", out var dryRun))
        {
            options.DryRun = dryRun;
        }

        if (TryGetBool(root, "ExportJson", out var exportJson))
        {
            options.ExportJson = exportJson;
        }

        if (TryGetBool(root, "ExportCsv", out var exportCsv))
        {
            options.ExportCsv = exportCsv;
        }

        if (TryGetString(root, "OutputPath", out var outputPath))
        {
            options.OutputPath = outputPath!;
        }

        if (TryGetString(root, "JsonFileName", out var jsonFileName))
        {
            options.JsonFileName = jsonFileName!;
        }

        if (TryGetString(root, "CsvFileName", out var csvFileName))
        {
            options.CsvFileName = csvFileName!;
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

    private static bool TryGetDouble(JsonElement element, string name, out double value)
    {
        if (element.TryGetProperty(name, out var property) &&
            property.ValueKind == JsonValueKind.Number &&
            property.TryGetDouble(out value))
        {
            return true;
        }

        value = default;
        return false;
    }
}
