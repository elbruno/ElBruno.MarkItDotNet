#:sdk Aspire.AppHost.Sdk@13.4.3

var builder = DistributedApplication.CreateBuilder(args);

var agent = builder.AddProject("markitdotnet-agent", "./Agent/MarkItDotNet.FoundryHostedAgent.csproj")
    .WithHttpEndpoint(env: "PORT")
    .WithHttpHealthCheck("/health");

builder.AddProject("markitdotnet-webapp", "./WebUi/MarkItDotNet.FoundryHostedAgent.WebUi.csproj")
    .WithHttpEndpoint()
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithReference(agent)
    .WaitFor(agent);

builder.Build().Run();
