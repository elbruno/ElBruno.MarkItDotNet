using ElBruno.Whisper;

namespace ElBruno.MarkItDotNet.Whisper;

/// <summary>
/// Options for configuring the Whisper audio converter.
/// </summary>
public class WhisperOptions
{
    /// <summary>
    /// The Whisper model to use. Defaults to null (uses WhisperClient defaults — tiny.en).
    /// Use <see cref="KnownWhisperModels"/> constants for available models.
    /// </summary>
    public WhisperModelDefinition? Model { get; set; }
}
