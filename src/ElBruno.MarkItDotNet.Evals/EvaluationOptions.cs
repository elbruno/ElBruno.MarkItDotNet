namespace ElBruno.MarkItDotNet.Evals;

/// <summary>
/// Options controlling evaluation behavior.
/// </summary>
public sealed class EvaluationOptions
{
    /// <summary>
    /// Gets or sets the minimum acceptable evaluation score.
    /// </summary>
    public double PassThreshold { get; set; } = 0.70;
}
