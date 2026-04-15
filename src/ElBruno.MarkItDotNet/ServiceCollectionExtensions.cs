using ElBruno.MarkItDotNet.Converters;
using Microsoft.Extensions.DependencyInjection;

namespace ElBruno.MarkItDotNet;

/// <summary>
/// Extension methods for registering MarkItDotNet services with the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MarkItDotNet services (ConverterRegistry, MarkdownService, built-in converters) to the service collection.
    /// </summary>
    public static IServiceCollection AddMarkItDotNet(
        this IServiceCollection services,
        Action<MarkItDotNetOptions>? configure = null)
    {
        var options = new MarkItDotNetOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);

        var registry = new ConverterRegistry();

        // Register built-in converters
        registry.Register(new PlainTextConverter());
        registry.Register(new JsonConverter());
        registry.Register(new HtmlConverter());
        registry.Register(new DocxConverter());
        registry.Register(new PdfConverter());
        registry.Register(new ImageConverter());
        registry.Register(new CsvConverter());
        registry.Register(new XmlConverter());
        registry.Register(new YamlConverter());
        registry.Register(new RtfConverter());
        registry.Register(new EpubConverter());
        registry.Register(new UrlConverter());
        registry.Register(new MarkdownPassthroughConverter());

        services.AddSingleton(registry);
        services.AddSingleton<MarkdownService>(sp =>
        {
            var reg = sp.GetRequiredService<ConverterRegistry>();
            var plugins = sp.GetServices<IConverterPlugin>();
            var opts = sp.GetRequiredService<MarkItDotNetOptions>();
            return new MarkdownService(reg, plugins, opts);
        });

        return services;
    }
}
