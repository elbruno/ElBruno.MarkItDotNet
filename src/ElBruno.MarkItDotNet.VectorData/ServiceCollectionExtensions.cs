// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;

namespace ElBruno.MarkItDotNet.VectorData;

/// <summary>
/// Extension methods for registering vector data services with the dependency injection container.
/// </summary>
public static class VectorDataServiceCollectionExtensions
{
    /// <summary>
    /// Adds MarkItDotNet vector data services to the specified <see cref="IServiceCollection"/>.
    /// Registers <see cref="IVectorRecordMapper"/> with the <see cref="DefaultVectorRecordMapper"/> implementation.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddMarkItDotNetVectorData(this IServiceCollection services)
    {
        services.AddSingleton<IVectorRecordMapper, DefaultVectorRecordMapper>();
        return services;
    }
}
