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
using Squidex.Infrastructure.ObjectPool;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2
{
    public static class ResolverExtensions
    {
        private static readonly string[] QueryFields = { "top", "skip", "filter", "orderby" };

        public static Context RequestContext(this IResolverContext context)
        {
            return context.GetGlobalValue<Context>(nameof(RequestContext))!;
        }

        public static DomainId Id(this IResolverContext context)
        {
            return DomainId.Create(context.ArgumentValue<string>("id"));
        }

        public static IObjectFieldDescriptor UseId(this IObjectFieldDescriptor field)
        {
            field.Argument("id", arg => arg
                .Type<NonNullType<StringType>>()
                .Description("The id of the item."));

            return field;
        }

        public static string SourceName(this IResolverContext context)
        {
            return (string)context.Field.ContextData[nameof(SourceName)]!;
        }

        public static IObjectFieldDescriptor WithSourceName(this IObjectFieldDescriptor field, string name)
        {
            field.ConfigureContextData(data => data[nameof(SourceName)] = name);

            return field;
        }

        public static string SchemaId(this IResolverContext context)
        {
            return (string)context.Field.ContextData[nameof(SchemaId)]!;
        }

        public static IObjectFieldDescriptor UseSchemaId(this IObjectFieldDescriptor field, string name)
        {
            field.ConfigureContextData(data => data[nameof(SchemaId)] = name);

            return field;
        }

        public static IDataLoader<DomainId, IEnrichedAssetEntity> AssetDataLoader(this IResolverContext context)
        {
            var assetQuery = context.Service<IAssetQueryService>();

            var requestContext = context.RequestContext();

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

            var requestContext = context.RequestContext();

            var dataLoader = context.BatchDataLoader<DomainId, IEnrichedContentEntity>(async (keys, ct) =>
            {
                var query = Q.Empty.WithIds(keys);

                var contents = await contentQuery.QueryAsync(requestContext, query);

                return contents.ToDictionary(x => x.Id);
            }, "contentsbyIds");

            return dataLoader;
        }

        public static IObjectFieldDescriptor UseODataArgs(this IObjectFieldDescriptor descriptor)
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

        public static string BuildODataQuery(this IResolverContext context)
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