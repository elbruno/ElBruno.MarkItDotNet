using ElBruno.Whisper;

namespace ElBruno.MarkItDotNet.Whisper;

/// <summary>
/// Converts audio files to Markdown using local Whisper speech-to-text.
/// Supports: .wav, .mp3, .m4a, .ogg, .flac.
/// Uses ONNX Runtime — no cloud API needed.
/// </summary>
public class WhisperAudioConverter : IMarkdownConverter
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".wav", ".mp3", ".m4a", ".ogg", ".flac"
    };

    private readonly WhisperClient _client;

    /// <summary>
    /// Initializes a new instance of <see cref="WhisperAudioConverter"/> with the specified Whisper client.
    /// </summary>
    /// <param name="client">The Whisper client used for speech-to-text transcription.</param>
    public WhisperAudioConverter(WhisperClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
    }

    /// <inheritdoc />
    public bool CanHandle(string fileExtension) =>
        SupportedExtensions.Contains(fileExtension);

    /// <inheritdoc />
    public async Task<string> ConvertAsync(Stream fileStream, string fileExtension, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        // WhisperClient needs a file path, so we write to a temp file
        var tempDir = Path.Combine(Path.GetTempPath(), "markitdotnet");
        Directory.CreateDirectory(tempDir);
        var tempPath = Path.Combine(tempDir, $"whisper-{Guid.NewGuid()}{fileExtension}");
        try
        {
            using (var fs = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await fileStream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
            }

            var result = await _client.TranscribeAsync(tempPath).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(result.Text))
                return "*No speech detected in audio file.*";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("# Audio Transcription");
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(result.DetectedLanguage))
            {
                sb.AppendLine($"**Language:** {result.DetectedLanguage}  ");
            }

            if (result.Duration != default)
            {
                sb.AppendLine($"**Duration:** {result.Duration:hh\\:mm\\:ss}  ");
            }

            sb.AppendLine();
            sb.AppendLine(result.Text.Trim());
            sb.AppendLine();

            return sb.ToString();
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { /* best effort cleanup */ }
            }
        }
    }
}
