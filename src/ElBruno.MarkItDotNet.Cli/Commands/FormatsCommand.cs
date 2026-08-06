using ElBruno.MarkItDotNet.Excel;
using ElBruno.MarkItDotNet.PowerPoint;
using Microsoft.Extensions.DependencyInjection;

namespace ElBruno.MarkItDotNet.Cli.Commands;

/// <summary>
/// Handler for: markitdown formats
/// </summary>
internal static class FormatsCommand
{
    public static Task<int> HandleAsync()
    {
        var services = new ServiceCollection();
        services.AddMarkItDotNet();
        services.AddMarkItDotNetExcel();
        services.AddMarkItDotNetPowerPoint();
        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<ConverterRegistry>();

        var converters = registry.GetAll();

        Console.WriteLine("Supported formats:");
        Console.WriteLine();

        var knownExtensions = new[]
        {
            ".txt", ".log", ".md",
            ".json",
            ".html", ".htm",
            ".docx",
            ".pdf",
            ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".tiff", ".webp",
            ".csv", ".tsv",
            ".xml",
            ".yaml", ".yml",
            ".rtf",
            ".epub",
            ".url",
            ".xlsx",
            ".pptx"
        };

        foreach (var converter in converters)
        {
            var supported = knownExtensions
                .Where(ext => converter.CanHandle(ext))
                .ToList();

            if (supported.Count > 0)
            {
                var typeName = converter.GetType().Name;
                Console.WriteLine($"  {typeName,-28} {string.Join(", ", supported)}");
            }
        }

        return Task.FromResult(0);
    }
}
