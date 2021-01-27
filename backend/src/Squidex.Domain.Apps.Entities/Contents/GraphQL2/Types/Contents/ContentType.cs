// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents
{
    internal sealed class ContentType : ObjectType<IEnrichedContentEntity>
    {
        private readonly GraphQLSchemaBuilder builder;
        private readonly ISchemaEntity schema;
        private readonly string schemaType;
        private readonly string schemaName;

        public ContentType(GraphQLSchemaBuilder builder, ISchemaEntity schema)
        {
            this.builder = builder;
            this.schema = schema;
            this.schemaType = schema.TypeName();
            this.schemaName = schema.DisplayName();
        }

        protected override void Configure(IObjectTypeDescriptor<IEnrichedContentEntity> descriptor)
        {
            descriptor.Field(x => x.Id)
                .Type<NonNullType<StringType>>()
                .Description("The id of the content.");

            descriptor.Field(x => x.Version)
               .Type<NonNullType<LongType>>()
               .Description("The version of the content.");

            descriptor.Field(x => x.Created)
                .Type<NonNullType<DateTimeType>>()
                .Description("The date and time when the content has been created.");

            descriptor.Field(x => x.CreatedBy)
                .Type<NonNullType<StringType>>()
                .Description("The user that has created the content.");

            descriptor.Field(x => x.LastModified)
                .Type<NonNullType<DateTimeType>>()
                .Description("The date and time when the content has been modified last.");

            descriptor.Field(x => x.LastModifiedBy)
                .Type<NonNullType<StringType>>()
                .Description("The user that has updated the content last.");

            descriptor.Field(x => x.Status).Resolve(Resolver(x => x.Status.ToString()))
                .Type<NonNullType<StringType>>()
                .Description("The status.");

            descriptor.Field(x => x.StatusColor)
                .Type<NonNullType<StringType>>()
                .Description("The status color.");

            descriptor.Field(x => x.Data)
                .Type(new NonNullType(new ContentDataType(builder, schema, schemaName, schemaType)))
                .Description("The content data.");

            descriptor.Field("flatData").ResolveWith<IEnrichedContentEntity>(x => x.Data)
                .Type(new NonNullType(new FlatDataType(builder, schema, schemaName, schemaType)))
                .Description("The flat content data.");
        }

        private static Func<IResolverContext, T> Resolver<T>(Func<IEnrichedContentEntity, T> resolver)
        {
            return context => resolver(context.Parent<IEnrichedContentEntity>());
        }
    }
}
