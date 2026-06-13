namespace ElBruno.MarkItDotNet.Evals;

/// <summary>
/// Evaluates conversion output quality and readiness.
/// </summary>
public interface IEvaluationEngine
{
    /// <summary>
    /// Evaluates a conversion result, optionally comparing to source text.
    /// </summary>
    /// <param name="result">The conversion result to evaluate.</param>
    /// <param name="sourceText">Optional raw source text used for rough retention checks.</param>
    /// <returns>An <see cref="EvaluationReport"/>.</returns>
    EvaluationReport Evaluate(ConversionResult result, string? sourceText = null);
}
