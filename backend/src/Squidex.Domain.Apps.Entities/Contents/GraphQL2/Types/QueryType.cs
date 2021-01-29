// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Assets;
using Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types
{
    public class QueryType : ObjectType
    {
        private readonly IEnumerable<SchemaType> schemas;

        public QueryType(IEnumerable<SchemaType> schemas)
        {
            this.schemas = schemas;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            foreach (var schema in schemas)
            {
                ConfigureSchema(descriptor, schema);
            }

            ConfigureAssets(descriptor);
        }

        private static void ConfigureSchema(IObjectTypeDescriptor descriptor, SchemaType schemaType)
        {
            descriptor.Field($"find{schemaType.TypeName}Content").Resolve(FindContent)
                .Type(new NamedTypeNode(schemaType.ContentType))
                .UseSchemaId(schemaType.Schema.Id)
                .UseId()
                .Argument("version", arg => arg
                    .Type<IntType>()
                    .Description("The version of the content"))
                .Description($"Find an {schemaType.DisplayName} content by id.");

            descriptor.Field($"query{schemaType.TypeName}Contents").Resolve(QueryContents)
                .Type(new NonNullTypeNode(new ListTypeNode(new NonNullTypeNode(new NamedTypeNode(schemaType.ContentType)))))
                .UseSchemaId(schemaType.Schema.Id)
                .UseODataArgs()
                .Description($"Query {schemaType.TypeName} content items.");

            descriptor.Field($"query{schemaType.TypeName}ContentsWithTotal").Resolve(QueryContents)
                .Type(new NonNullTypeNode(new NamedTypeNode(schemaType.ResultType)))
                .UseSchemaId(schemaType.Schema.Id)
                .UseODataArgs()
                .Description($"Query {schemaType.TypeName} content items with total count.");
        }

        private static void ConfigureAssets(IObjectTypeDescriptor descriptor)
        {
            descriptor.Field("findAsset").Resolve(FindAsset)
                .Type<AssetType>()
                .UseId()
                .Description("Find an asset by id");

            descriptor.Field("queryAssets").Resolve(QueryAssets)
                .Type<NonNullType<ListType<NonNullType<AssetType>>>>()
                .UseODataArgs()
                .Description("Query assets.");

            descriptor.Field("queryAssetsWithTotal").Resolve(QueryAssets)
                .Type<NonNullType<AssetResultType>>()
                .UseODataArgs()
                .Description("Query assets with the total count.");
        }

        private static readonly FieldResolverDelegate FindAsset = async context =>
        {
            var dataLoader = context.AssetDataLoader();

            return await dataLoader.LoadAsync(context.Id(), default);
        };

        private static readonly FieldResolverDelegate FindContent = async context =>
        {
            var version = context.ArgumentValue<int?>("version");

            if (version.HasValue)
            {
                var contentQuery = context.Service<IContentQueryService>();

                return await contentQuery.FindAsync(context.RequestContext(), context.SchemaId(), context.Id(), version.Value);
            }
            else
            {
                var dataLoader = context.ContentDataLoader();

                return await dataLoader.LoadAsync(context.Id(), default);
            }
        };

        private static readonly FieldResolverDelegate QueryAssets = async context =>
        {
            var assetQuery = context.Service<IAssetQueryService>();

            var q = Q.Empty.WithODataQuery(context.BuildODataQuery());

            return await assetQuery.QueryAsync(context.RequestContext(), null, q);
        };

        private static readonly FieldResolverDelegate QueryContents = async context =>
        {
            var contentQuery = context.Service<IContentQueryService>();

            var q = Q.Empty.WithODataQuery(context.BuildODataQuery());

            return await contentQuery.QueryAsync(context.RequestContext(), context.SchemaId(), q);
        };
    }
}