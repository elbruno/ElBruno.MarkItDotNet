using ElBruno.MarkItDotNet;
using ElBruno.MarkItDotNet.Whisper;
using ElBruno.Whisper;

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║  ElBruno.MarkItDotNet - Whisper Transcription Sample      ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");

// ── Option 1: Manual setup (no DI) ──────────────────────────────────────
Console.WriteLine("🎙️  SETUP: Creating Whisper client and converter...");
Console.WriteLine("─────────────────────────────────────────────────────────\n");

Console.WriteLine("  Step 1: Create WhisperClient (downloads model on first run)");
Console.WriteLine("    var client = await WhisperClient.CreateAsync();");
Console.WriteLine();

Console.WriteLine("  Step 2: Create the plugin and register with ConverterRegistry");
Console.WriteLine("    var plugin = new WhisperConverterPlugin(client);");
Console.WriteLine("    var registry = new ConverterRegistry();");
Console.WriteLine("    registry.RegisterPlugin(plugin);");
Console.WriteLine();

Console.WriteLine("  Step 3: Convert an audio file to Markdown");
Console.WriteLine("    var converter = registry.Resolve(\".wav\");");
Console.WriteLine("    using var stream = File.OpenRead(\"audio.wav\");");
Console.WriteLine("    var markdown = await converter.ConvertAsync(stream, \".wav\");");
Console.WriteLine();

// ── Option 2: Using DI ──────────────────────────────────────────────────
Console.WriteLine("🔧 ALTERNATIVE: Using Dependency Injection");
Console.WriteLine("─────────────────────────────────────────────────────────\n");

Console.WriteLine("  var services = new ServiceCollection();");
Console.WriteLine("  services.AddMarkItDotNet();");
Console.WriteLine("  services.AddMarkItDotNetWhisper(options =>");
Console.WriteLine("  {");
Console.WriteLine("      options.Model = KnownWhisperModels.WhisperBaseEn; // Optional: default is tiny.en");
Console.WriteLine("  });");
Console.WriteLine();

// ── Supported formats ────────────────────────────────────────────────────
Console.WriteLine("📁 SUPPORTED AUDIO FORMATS:");
Console.WriteLine("─────────────────────────────────────────────────────────\n");
string[] formats = [".wav", ".mp3", ".m4a", ".ogg", ".flac"];
foreach (var format in formats)
{
    Console.WriteLine($"  ✅ {format}");
}
Console.WriteLine();

// ── Live demo (if an audio file is provided) ─────────────────────────────
var audioPath = args.Length > 0 ? args[0] : null;

if (!string.IsNullOrEmpty(audioPath) && File.Exists(audioPath))
{
    Console.WriteLine($"🎵 LIVE DEMO: Transcribing '{Path.GetFileName(audioPath)}'...");
    Console.WriteLine("─────────────────────────────────────────────────────────\n");

    using var client = await WhisperClient.CreateAsync();
    var plugin = new WhisperConverterPlugin(client);
    var registry = new ConverterRegistry();
    registry.RegisterPlugin(plugin);

    var extension = Path.GetExtension(audioPath).ToLowerInvariant();
    var converter = registry.Resolve(extension);

    if (converter is not null)
    {
        using var stream = File.OpenRead(audioPath);
        var markdown = await converter.ConvertAsync(stream, extension);
        Console.WriteLine("✅ Transcription result:\n");
        Console.WriteLine(markdown);
    }
    else
    {
        Console.WriteLine($"❌ Unsupported format: {extension}");
    }
}
else
{
    Console.WriteLine("💡 TIP: Pass an audio file path as an argument to see live transcription:");
    Console.WriteLine("    dotnet run -- \"path/to/audio.wav\"");
    Console.WriteLine();
}

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║          Whisper Transcription Sample Complete!           ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
