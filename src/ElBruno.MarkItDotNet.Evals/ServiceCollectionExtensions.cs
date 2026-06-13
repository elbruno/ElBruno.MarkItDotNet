using Microsoft.Extensions.DependencyInjection;

namespace ElBruno.MarkItDotNet.Evals;

/// <summary>
/// Extension methods for registering MarkItDotNet evaluation services.
/// </summary>
public static class EvalsServiceCollectionExtensions
{
    /// <summary>
    /// Adds MarkItDotNet evaluation services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddMarkItDotNetEvals(
        this IServiceCollection services,
        Action<EvaluationOptions>? configure = null)
    {
        var options = new EvaluationOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<IEvaluationEngine, ConversionEvaluationEngine>();
        return services;
    }
}
