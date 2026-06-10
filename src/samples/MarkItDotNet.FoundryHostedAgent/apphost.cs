#:sdk Aspire.AppHost.Sdk@13.0.0

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject("markitdotnet-foundry-agent", "./MarkItDotNet.FoundryHostedAgent.csproj")
    .WithHttpEndpoint(env: "PORT");

builder.Build().Run();
