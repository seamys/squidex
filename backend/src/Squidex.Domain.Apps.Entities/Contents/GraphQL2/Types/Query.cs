// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Resolvers;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.ObjectPool;

#pragma warning disable CA1822 // Mark members as static

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types
{
    public sealed class Query
    {
        private static readonly string[] QueryFields = { "top", "skip", "filter", "orderby" };

        public Task<IEnrichedAssetEntity> FindAssetAsync(DomainId id, IResolverContext context)
        {
            var dataLoader = context.AssetDataLoader();

            return dataLoader.LoadAsync(id, default);
        }

        public Task<IEnrichedContentEntity> FindContentAsync(DomainId id, int? version, IResolverContext context)
        {
            var dataLoader = context.ContentDataLoader();

            return dataLoader.LoadAsync(id, default);
        }

        public Task<IResultList<IEnrichedAssetEntity>> QueryAssetsAsync(IResolverContext context,
            [Service] IAssetQueryService assetQuery)
        {
            var requestContext = context.GetGlobalValue<Context>("requestContext")!;

            var query = Q.Empty.WithODataQuery(BuildODataQuery(context));

            return assetQuery.QueryAsync(requestContext, null, query);
        }

        public Task<IResultList<IEnrichedContentEntity>> QueryContetnsAsync(IResolverContext context,
            [Service] IContentQueryService contentQuery)
        {
            var requestContext = context.GetGlobalValue<Context>("requestContext")!;

            var query = Q.Empty.WithODataQuery(BuildODataQuery(context));

            return contentQuery.QueryAsync(requestContext, context.ContextData["schemaId"]?.ToString()!, query);
        }

        public static string BuildODataQuery(IResolverContext context)
        {
            var sb = DefaultPools.StringBuilder.Get();
            try
            {
                sb.Append('?');

                foreach (var field in QueryFields)
                {
                    var value = context.ArgumentValue<object?>(field)?.ToString();

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        if (sb.Length > 1)
                        {
                            sb.Append('&');
                        }

                        sb.Append('$');
                        sb.Append(field);
                        sb.Append('=');
                        sb.Append(value);
                    }
                }

                return sb.ToString();
            }
            finally
            {
                DefaultPools.StringBuilder.Return(sb);
            }
        }
    }
}
