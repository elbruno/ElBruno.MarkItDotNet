namespace MarkItDotNet.FoundryHostedAgent.WebUi;

public sealed class AgentUiOptions
{
    public const string SectionName = "AgentUi";

    /// <summary>
    /// Aspire resource/service name for the hosted agent project.
    /// This is resolved via service discovery at runtime.
    /// </summary>
    public string AgentServiceName { get; set; } = "markitdotnet-agent";

    public long MaxUploadBytes { get; set; } = 20 * 1024 * 1024;
}
