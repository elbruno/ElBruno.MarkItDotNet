using ElBruno.MarkItDotNet.Excel;
using ElBruno.MarkItDotNet.PowerPoint;
using Microsoft.Extensions.DependencyInjection;

namespace ElBruno.MarkItDotNet.GoldenTests;

/// <summary>
/// Shared fixture that provides a configured MarkdownService with all converters registered.
/// </summary>
public class MarkdownServiceFixture : IDisposable
{
    public MarkdownService Service { get; }

    private readonly ServiceProvider _serviceProvider;

    public MarkdownServiceFixture()
    {
        var services = new ServiceCollection();
        services.AddMarkItDotNet();
        services.AddMarkItDotNetExcel();
        services.AddMarkItDotNetPowerPoint();
        _serviceProvider = services.BuildServiceProvider();
        Service = _serviceProvider.GetRequiredService<MarkdownService>();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}
