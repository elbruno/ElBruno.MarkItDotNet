namespace ElBruno.MarkItDotNet.Evals;

/// <summary>
/// Represents aggregate results for a conversion evaluation.
/// </summary>
/// <param name="Score">Overall score in range [0,1].</param>
/// <param name="Issues">Detected issues.</param>
/// <param name="Metrics">Named metrics for diagnostics.</param>
public sealed record EvaluationReport(
    double Score,
    IReadOnlyList<EvaluationIssue> Issues,
    IReadOnlyDictionary<string, double> Metrics)
{
    /// <summary>
    /// Gets a value indicating whether the evaluation passed the given threshold.
    /// </summary>
    /// <param name="threshold">Pass threshold in [0,1].</param>
    /// <returns><c>true</c> when <see cref="Score"/> meets or exceeds threshold.</returns>
    public bool Passes(double threshold) => Score >= threshold;
}
