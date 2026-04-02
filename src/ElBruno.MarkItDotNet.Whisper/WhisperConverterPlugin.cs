namespace ElBruno.MarkItDotNet.Whisper;

/// <summary>
/// Plugin that registers the Whisper audio converter for local speech-to-text.
/// </summary>
public class WhisperConverterPlugin : IConverterPlugin
{
    /// <inheritdoc />
    public string Name => "Whisper";

    private readonly ElBruno.Whisper.WhisperClient _client;

    /// <summary>
    /// Initializes a new instance of <see cref="WhisperConverterPlugin"/> with the specified Whisper client.
    /// </summary>
    /// <param name="client">The Whisper client used for speech-to-text transcription.</param>
    public WhisperConverterPlugin(ElBruno.Whisper.WhisperClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
    }

    /// <inheritdoc />
    public IEnumerable<IMarkdownConverter> GetConverters() =>
    [
        new WhisperAudioConverter(_client)
    ];
}
