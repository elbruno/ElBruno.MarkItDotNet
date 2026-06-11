#:sdk Aspire.AppHost.Sdk@13.4.3

var builder = DistributedApplication.CreateBuilder(args);

var agent = builder.AddProject("markitdotnet-foundry-agent", "./Agent/MarkItDotNet.FoundryHostedAgent.csproj")
    .WithHttpEndpoint(env: "PORT");

builder.AddProject("markitdotnet-foundry-agent-ui", "./WebUi/MarkItDotNet.FoundryHostedAgent.WebUi.csproj")
    .WithEnvironment("AgentUi__DefaultAgentUrl",
        ReferenceExpression.Create($"{agent.GetEndpoint("http")}/invocations"))
    .WaitFor(agent);

builder.Build().Run();
