// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using HotChocolate.Types;
using Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Assets;
using Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types
{
    public class QueryType : ObjectType<Query>
    {
        private readonly GraphQLSchemaBuilder builder;
        private readonly IEnumerable<ISchemaEntity> schemas;

        public QueryType()
        {
        }

        /*
        public QueryType(GraphQLSchemaBuilder builder, IEnumerable<ISchemaEntity> schemas)
        {
            this.builder = builder;
            this.schemas = schemas;
        }*/

        protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
        {
            ConfigureAssets(descriptor);
/*
            foreach (var schema in schemas)
            {
                // ConfigureSchema(descriptor, schema);
            }*/
        }

        private void ConfigureSchema(IObjectTypeDescriptor<Query> descriptor, ISchemaEntity schema)
        {
            var schemaId = schema.Id;
            var schemaType = schema.TypeName();
            var schemaName = schema.DisplayName();

            var contentType = builder.GetContentType(schema.Id);

            descriptor.Field($"find{schemaType}Content")
                .ResolveWith<Query>(x => x.FindContentAsync(default!, default!, default!))
                .Type(contentType)
                .ConfigureContextData(data => data.Add("schemaId", schemaId))
                .Argument("id", arg => arg
                    .Type<NonNullType<StringType>>()
                    .Description("The id of the content"))
                .Argument("version", arg => arg
                    .Type<IntType>()
                    .Description("The version of the content"))
                .Description($"Find an {schemaName} content by id.");

            descriptor.Field($"query{schemaType}Contents")
                .ResolveWith<Query>(x => x.QueryAssetsAsync(default!, default!))
                .Type(new NonNullType(new ListType(new NonNullType(contentType))))
                .ConfigureContextData(data => data.Add("schemaId", schemaId))
                .WithODataArgs()
                .Description("Query assets.");

            descriptor.Field($"query{schemaType}ContentsWithTotal")
                .ResolveWith<Query>(x => x.QueryAssetsAsync(default!, default!))
                .Type(new NonNullType(new ContentResultType(schemaName, schemaType, contentType)))
                .ConfigureContextData(data => data.Add("schemaId", schemaId))
                .WithODataArgs()
                .Description($"Query {schemaName} content items with total count.");
        }

        private static void ConfigureAssets(IObjectTypeDescriptor<Query> descriptor)
        {
            descriptor.Field("findAsset")
                .ResolveWith<Query>(x => x.FindAssetAsync(default!, default!))
                .Type<AssetType>()
                .Argument("id", arg => arg
                    .Type<NonNullType<StringType>>()
                    .Description("The id of the asset"))
                .Description("Find an asset by id");

            descriptor.Field("queryAssets")
                .ResolveWith<Query>(x => x.QueryAssetsAsync(default!, default!))
                .Type<NonNullType<ListType<NonNullType<AssetType>>>>()
                .WithODataArgs()
                .Description("Query assets.");

            descriptor.Field("queryAssetsWithTotal")
                .ResolveWith<Query>(x => x.QueryAssetsAsync(default!, default!))
                .Type<NonNullType<AssetResultType>>()
                .WithODataArgs()
                .Description("Query assets with the total count.");
        }
    }
}