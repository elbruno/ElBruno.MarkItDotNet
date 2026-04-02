using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Whisper.Tests;

public class WhisperAudioConverterTests
{
    [Theory]
    [InlineData(".wav")]
    [InlineData(".mp3")]
    [InlineData(".m4a")]
    [InlineData(".ogg")]
    [InlineData(".flac")]
    public void CanHandle_SupportedExtensions_ReturnsTrue(string extension)
    {
        // Verify the extension is in the SupportedExtensions set
        var field = typeof(WhisperAudioConverter)
            .GetField("SupportedExtensions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        field.Should().NotBeNull("SupportedExtensions field should exist");

        var hashSet = field!.GetValue(null) as HashSet<string>;
        hashSet.Should().NotBeNull();
        hashSet!.Contains(extension).Should().BeTrue($"extension '{extension}' should be supported");
    }

    [Theory]
    [InlineData(".wav")]
    [InlineData(".WAV")]
    [InlineData(".mp3")]
    [InlineData(".MP3")]
    [InlineData(".m4a")]
    [InlineData(".ogg")]
    [InlineData(".flac")]
    public void SupportedExtensions_AreCaseInsensitive(string extension)
    {
        // Verify the supported extensions set is defined with OrdinalIgnoreCase
        var field = typeof(WhisperAudioConverter)
            .GetField("SupportedExtensions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        field.Should().NotBeNull("SupportedExtensions field should exist");

        var hashSet = field!.GetValue(null) as HashSet<string>;
        hashSet.Should().NotBeNull();
        hashSet!.Contains(extension).Should().BeTrue($"extension '{extension}' should be supported (case-insensitive)");
    }

    [Theory]
    [InlineData(".docx")]
    [InlineData(".pdf")]
    [InlineData(".txt")]
    [InlineData(".xlsx")]
    [InlineData(".pptx")]
    [InlineData(".csv")]
    [InlineData(".avi")]
    [InlineData(".mp4")]
    public void SupportedExtensions_DoNotContainUnsupported(string extension)
    {
        var field = typeof(WhisperAudioConverter)
            .GetField("SupportedExtensions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var hashSet = field!.GetValue(null) as HashSet<string>;
        hashSet.Should().NotBeNull();
        hashSet!.Contains(extension).Should().BeFalse($"extension '{extension}' should not be supported");
    }

    [Fact]
    public void Constructor_NullClient_ThrowsArgumentNullException()
    {
        var act = () => new WhisperAudioConverter(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Fact]
    public void ImplementsIMarkdownConverter()
    {
        typeof(WhisperAudioConverter).Should().Implement<IMarkdownConverter>();
    }
}
