// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using GreenDonut;
using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents
{
    public static class ResolverExtensions
    {
        public static IDataLoader<DomainId, IEnrichedAssetEntity> AssetDataLoader(this IResolverContext context)
        {
            var assetQuery = context.Service<IAssetQueryService>();

            var requestContext = context.GetGlobalValue<Context>("requestContext")!;

            var dataLoader = context.BatchDataLoader<DomainId, IEnrichedAssetEntity>(async (keys, ct) =>
            {
                var query = Q.Empty.WithIds(keys);

                var assets = await assetQuery.QueryAsync(requestContext, null, query);

                return assets.ToDictionary(x => x.Id);
            }, "assetByIds");

            return dataLoader;
        }

        public static IDataLoader<DomainId, IEnrichedContentEntity> ContentDataLoader(this IResolverContext context)
        {
            var contentQuery = context.Service<IContentQueryService>();

            var requestContext = context.GetGlobalValue<Context>("requestContext")!;

            var dataLoader = context.BatchDataLoader<DomainId, IEnrichedContentEntity>(async (keys, ct) =>
            {
                var query = Q.Empty.WithIds(keys);

                var contents = await contentQuery.QueryAsync(requestContext, query);

                return contents.ToDictionary(x => x.Id);
            }, "contentsbyIds");

            return dataLoader;
        }

        public static IObjectFieldDescriptor WithODataArgs(this IObjectFieldDescriptor descriptor)
        {
            return descriptor
                .Argument("top", arg => arg
                    .Type<IntType>()
                    .Description("Optional number of items to take."))
                .Argument("skip", arg => arg
                    .Type<IntType>()
                    .Description("Optional number of items to skip."))
                .Argument("filter", arg => arg
                    .Type<StringType>()
                    .Description("Optional OData filter."))
                .Argument("orderby", arg => arg
                    .Type<StringType>()
                    .Description("Optional OData ordering."));
        }
    }
}
