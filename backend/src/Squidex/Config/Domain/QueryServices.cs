// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using GraphQL.DataLoader;
using HotChocolate.Execution;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;
using Squidex.Domain.Apps.Entities.Contents.GraphQL2;
using Squidex.Web.GraphQL;
using Squidex.Web.Services;

namespace Squidex.Config.Domain
{
    public static class QueryServices
    {
        public static void AddSquidexQueries(this IServiceCollection services, IConfiguration config)
        {
            var exposeSourceUrl = config.GetOptionalValue("assetStore:exposeSourceUrl", true);

            services.AddSingletonAs(c => ActivatorUtilities.CreateInstance<UrlGenerator>(c, exposeSourceUrl))
                .As<IUrlGenerator>();

            services.AddSingletonAs<DataLoaderContextAccessor>()
                .As<IDataLoaderContextAccessor>();

            services.AddSingletonAs<DataLoaderDocumentListener>()
                .AsSelf();

            services.AddSingletonAs<GraphQLTypeFactory>()
                .AsSelf();

            services.AddGraphQLServer()
                .ConfigureTypes();

            services.AddSingletonAs<GraphQLService>()
                .AsSelf();

            services.AddSingletonAs<AppRequestExecutorOptionsMonitor>()
                .As<IRequestExecutorOptionsMonitor>();

            services.AddSingletonWrapper<IRequestExecutorResolver, CachingRequestExecutorResolver>();
        }

        private static IServiceCollection AddSingletonWrapper<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            var existing = services.First(x => x.ServiceType == typeof(TInterface));

            if (existing == null)
            {
                services.AddSingleton<TInterface, TImplementation>();
            }
            else if (existing.ImplementationType != null)
            {
                services.AddSingleton<TInterface>(c =>
                {
                    var inner = (TInterface)ActivatorUtilities.CreateInstance(c, existing.ImplementationType);

                    return ActivatorUtilities.CreateInstance<TImplementation>(c, inner);
                });
            }
            else if (existing.ImplementationFactory != null)
            {
                services.AddSingleton<TInterface>(c =>
                {
                    var inner = existing.ImplementationFactory(c);

                    return ActivatorUtilities.CreateInstance<TImplementation>(c, inner);
                });
            }

            return services;
        }
    }
}