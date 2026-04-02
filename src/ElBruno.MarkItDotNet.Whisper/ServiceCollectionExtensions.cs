using ElBruno.Whisper;
using Microsoft.Extensions.DependencyInjection;

namespace ElBruno.MarkItDotNet.Whisper;

/// <summary>
/// Extension methods for registering the Whisper audio converter with dependency injection.
/// </summary>
public static class WhisperServiceCollectionExtensions
{
    /// <summary>
    /// Adds Whisper-based local audio transcription to the MarkItDotNet converter registry.
    /// Call this after <c>AddMarkItDotNet()</c>.
    /// </summary>
    public static IServiceCollection AddMarkItDotNetWhisper(
        this IServiceCollection services,
        Action<WhisperOptions>? configure = null)
    {
        var options = new WhisperOptions();
        configure?.Invoke(options);

        // Register WhisperClient as a factory — creates asynchronously on first resolve
        services.AddSingleton(sp =>
        {
            var whisperOptions = new ElBruno.Whisper.WhisperOptions();
            if (options.Model is not null)
            {
                whisperOptions.Model = options.Model;
            }

            // This blocks on async, but DI factories don't support async
            return WhisperClient.CreateAsync(whisperOptions).GetAwaiter().GetResult();
        });

        services.AddSingleton<WhisperAudioConverter>();
        services.AddSingleton<WhisperConverterPlugin>();

        // Register the plugin with the converter registry if available
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ConverterRegistry));
        if (descriptor?.ImplementationInstance is ConverterRegistry)
        {
            services.AddSingleton<IConverterPlugin>(sp => sp.GetRequiredService<WhisperConverterPlugin>());
        }

        return services;
    }
}
