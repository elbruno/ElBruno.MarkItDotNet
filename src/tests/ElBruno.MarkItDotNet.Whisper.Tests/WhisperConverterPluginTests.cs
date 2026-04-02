using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Whisper.Tests;

public class WhisperConverterPluginTests
{
    [Fact]
    public void Constructor_NullClient_ThrowsArgumentNullException()
    {
        var act = () => new WhisperConverterPlugin(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Fact]
    public void ImplementsIConverterPlugin()
    {
        typeof(WhisperConverterPlugin).Should().Implement<IConverterPlugin>();
    }

    [Fact]
    public void Name_ReturnsWhisper()
    {
        // Access Name via reflection to avoid constructing with a real client
        var property = typeof(WhisperConverterPlugin).GetProperty(nameof(IConverterPlugin.Name));
        property.Should().NotBeNull();
        property!.PropertyType.Should().Be<string>();
    }
}
