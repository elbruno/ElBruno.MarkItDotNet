// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ElBruno.MarkItDotNet.Sync.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMarkItDotNetSync_RegistersISyncStateStore_WithDefaultImplementation()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetSync();

        using var provider = services.BuildServiceProvider();
        provider.GetService<ISyncStateStore>().Should().NotBeNull()
            .And.BeOfType<InMemorySyncStateStore>();
    }

    [Fact]
    public void AddMarkItDotNetSync_RegistersSyncExecutor()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetSync();

        using var provider = services.BuildServiceProvider();
        provider.GetService<SyncExecutor>().Should().NotBeNull();
    }

    [Fact]
    public void AddMarkItDotNetSync_WithCustomStore_RegistersCustomISyncStateStore()
    {
        var services = new ServiceCollection();
        var customStore = new InMemorySyncStateStore();

        services.AddMarkItDotNetSync(sp => customStore);

        using var provider = services.BuildServiceProvider();
        provider.GetService<ISyncStateStore>().Should().BeSameAs(customStore);
    }

    [Fact]
    public void AddMarkItDotNetSync_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddMarkItDotNetSync();

        result.Should().BeSameAs(services);
    }
}
