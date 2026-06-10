FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet publish MarkItDotNet.FoundryHostedAgent.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8088
EXPOSE 8088

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MarkItDotNet.FoundryHostedAgent.dll"]
