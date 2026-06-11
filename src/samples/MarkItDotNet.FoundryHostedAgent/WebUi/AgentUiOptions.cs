namespace MarkItDotNet.FoundryHostedAgent.WebUi;

public sealed class AgentUiOptions
{
    public const string SectionName = "AgentUi";

    /// <summary>
    /// Default agent invocations URL shown in the textbox.
    /// Overridden at runtime by Aspire via AgentUi__DefaultAgentUrl when running locally,
    /// or by environment variable when deployed to Azure Container Apps.
    /// </summary>
    public string DefaultAgentUrl { get; set; } = "http://localhost:8088/invocations";

    public long MaxUploadBytes { get; set; } = 20 * 1024 * 1024;
}
