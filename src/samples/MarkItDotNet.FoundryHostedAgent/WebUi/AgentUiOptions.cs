namespace MarkItDotNet.FoundryHostedAgent.WebUi;

public sealed class AgentUiOptions
{
    public const string SectionName = "AgentUi";
    public const string DefaultServiceDiscoveryUrl = "http+https://markitdotnet-agent";

    /// <summary>
    /// Default agent URL shown in the textbox.
    /// Under Aspire, this is the service discovery name for the backend resource.
    /// Override with an absolute URL when running standalone or targeting a cloud endpoint.
    /// </summary>
    public string DefaultAgentUrl { get; set; } = DefaultServiceDiscoveryUrl;

    public long MaxUploadBytes { get; set; } = 20 * 1024 * 1024;
}
