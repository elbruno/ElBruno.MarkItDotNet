namespace ElBruno.MarkItDotNet;

/// <summary>
/// Registry of <see cref="IMarkdownConverter"/> implementations.
/// Resolves the appropriate converter for a given file extension.
/// <para>
/// ⚠️ Thread Safety: This class is not thread-safe for concurrent registration.
/// Register all converters during startup before resolving. Once configured,
/// <see cref="Resolve"/> and <see cref="GetAll"/> are safe for concurrent reads.
/// </para>
/// </summary>
public class ConverterRegistry
{
    private readonly List<IMarkdownConverter> _converters = [];
    private readonly List<IConverterPlugin> _plugins = [];

    /// <summary>
    /// Registers a converter in the registry.
    /// </summary>
    public void Register(IMarkdownConverter converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        _converters.Add(converter);
    }

    /// <summary>
    /// Registers all converters provided by a plugin.
    /// </summary>
    /// <param name="plugin">The plugin whose converters should be registered.</param>
    public void RegisterPlugin(IConverterPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        _plugins.Add(plugin);

        foreach (var converter in plugin.GetConverters())
        {
            _converters.Add(converter);
        }
    }

    /// <summary>
    /// Returns all registered plugins.
    /// </summary>
    public IReadOnlyList<IConverterPlugin> GetPlugins() => _plugins.AsReadOnly();

    /// <summary>
    /// Resolves the first converter that can handle the given file extension.
    /// </summary>
    /// <param name="extension">File extension including the leading dot (e.g., ".txt").</param>
    /// <returns>A matching converter, or null if none can handle the extension.</returns>
    public IMarkdownConverter? Resolve(string extension)
    {
        var normalized = extension.ToLowerInvariant();
        return _converters.FirstOrDefault(c => c.CanHandle(normalized));
    }

    /// <summary>
    /// Returns all registered converters.
    /// </summary>
    public IReadOnlyList<IMarkdownConverter> GetAll() => _converters.AsReadOnly();
}
