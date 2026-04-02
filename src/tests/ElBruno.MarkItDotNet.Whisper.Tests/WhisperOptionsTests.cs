using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Whisper.Tests;

public class WhisperOptionsTests
{
    [Fact]
    public void Model_DefaultsToNull()
    {
        var options = new WhisperOptions();
        options.Model.Should().BeNull();
    }

    [Fact]
    public void Model_CanBeSet()
    {
        var options = new WhisperOptions
        {
            Model = ElBruno.Whisper.KnownWhisperModels.WhisperBaseEn
        };
        options.Model.Should().NotBeNull();
        options.Model!.Id.Should().NotBeNullOrEmpty();
    }
}
