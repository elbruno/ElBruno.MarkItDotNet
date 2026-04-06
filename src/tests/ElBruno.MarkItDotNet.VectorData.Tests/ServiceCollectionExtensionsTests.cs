// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ElBruno.MarkItDotNet.VectorData.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMarkItDotNetVectorData_RegistersIVectorRecordMapper()
    {
        var services = new ServiceCollection();

        services.AddMarkItDotNetVectorData();

        using var provider = services.BuildServiceProvider();
        provider.GetService<IVectorRecordMapper>().Should().NotBeNull()
            .And.BeOfType<DefaultVectorRecordMapper>();
    }

    [Fact]
    public void AddMarkItDotNetVectorData_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddMarkItDotNetVectorData();

        result.Should().BeSameAs(services);
    }
}
