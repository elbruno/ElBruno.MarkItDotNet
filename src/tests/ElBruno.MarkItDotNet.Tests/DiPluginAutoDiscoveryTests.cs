using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests;

public class DiPluginAutoDiscoveryTests
{
    [Fact]
    public void MarkdownService_WithPluginsConstructor_RegistersPluginsIntoRegistry()
    {
        var registry = new ConverterRegistry();
        registry.Register(new PlainTextConverter());

        var plugin = new TestPlugin("TestPlugin", new TestConverter(".test"));
        var plugins = new[] { plugin };

        var service = new MarkdownService(registry, plugins);

        registry.GetPlugins().Should().ContainSingle(p => p.Name == "TestPlugin");
        registry.Resolve(".test").Should().NotBeNull();
    }

    [Fact]
    public void MarkdownService_WithNullPlugins_DoesNotThrow()
    {
        var registry = new ConverterRegistry();

        var act = () => new MarkdownService(registry, null);

        act.Should().NotThrow();
    }

    [Fact]
    public void MarkdownService_WithDuplicatePlugins_RegistersOnlyOnce()
    {
        var registry = new ConverterRegistry();
        var plugin = new TestPlugin("TestPlugin", new TestConverter(".test"));

        // Pre-register the plugin
        registry.RegisterPlugin(plugin);

        // Pass same plugin via constructor
        var service = new MarkdownService(registry, new[] { plugin });

        registry.GetPlugins().Should().HaveCount(1);
    }

    [Fact]
    public void AddMarkItDotNet_WithConverterPlugin_AutoDiscoveredByMarkdownService()
    {
        var services = new ServiceCollection();

        // Register core services
        services.AddMarkItDotNet();

        // Register a plugin via DI (simulating what AddMarkItDotNetAI does)
        services.AddSingleton<IConverterPlugin>(
            new TestPlugin("TestPlugin", new TestConverter(".test")));

        var provider = services.BuildServiceProvider();
        var markdownService = provider.GetRequiredService<MarkdownService>();
        var registry = provider.GetRequiredService<ConverterRegistry>();

        // The plugin should have been auto-discovered and registered
        registry.GetPlugins().Should().ContainSingle(p => p.Name == "TestPlugin");
        registry.Resolve(".test").Should().NotBeNull();
    }

    [Fact]
    public void AddMarkItDotNet_WithMultiplePlugins_AllDiscovered()
    {
        var services = new ServiceCollection();
        services.AddMarkItDotNet();

        services.AddSingleton<IConverterPlugin>(
            new TestPlugin("Plugin1", new TestConverter(".aaa")));
        services.AddSingleton<IConverterPlugin>(
            new TestPlugin("Plugin2", new TestConverter(".bbb")));

        var provider = services.BuildServiceProvider();
        var markdownService = provider.GetRequiredService<MarkdownService>();
        var registry = provider.GetRequiredService<ConverterRegistry>();

        registry.GetPlugins().Should().HaveCount(2);
        registry.Resolve(".aaa").Should().NotBeNull();
        registry.Resolve(".bbb").Should().NotBeNull();
    }

    [Fact]
    public void AddMarkItDotNet_WithNoPlugins_WorksNormally()
    {
        var services = new ServiceCollection();
        services.AddMarkItDotNet();

        var provider = services.BuildServiceProvider();
        var markdownService = provider.GetRequiredService<MarkdownService>();
        var registry = provider.GetRequiredService<ConverterRegistry>();

        // Built-in converters should still work
        registry.Resolve(".txt").Should().NotBeNull();
        registry.GetPlugins().Should().BeEmpty();
    }

    [Fact]
    public void BackwardCompatibility_SingleArgConstructor_StillWorks()
    {
        var registry = new ConverterRegistry();
        registry.Register(new PlainTextConverter());

        var service = new MarkdownService(registry);

        // Should work without plugins
        service.Should().NotBeNull();
    }

    private sealed class TestPlugin : IConverterPlugin
    {
        private readonly IMarkdownConverter[] _converters;

        public TestPlugin(string name, params IMarkdownConverter[] converters)
        {
            Name = name;
            _converters = converters;
        }

        public string Name { get; }

        public IEnumerable<IMarkdownConverter> GetConverters() => _converters;
    }

    private sealed class TestConverter : IMarkdownConverter
    {
        private readonly string _extension;

        public TestConverter(string extension)
        {
            _extension = extension;
        }

        public bool CanHandle(string fileExtension) =>
            fileExtension.Equals(_extension, StringComparison.OrdinalIgnoreCase);

        public Task<string> ConvertAsync(Stream fileStream, string fileExtension, CancellationToken cancellationToken = default)
        {
            return Task.FromResult("test-converted");
        }
    }
}
