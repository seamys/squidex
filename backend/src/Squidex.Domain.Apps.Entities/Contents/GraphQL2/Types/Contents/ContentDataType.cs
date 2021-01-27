// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents
{
    public sealed class ContentDataType : ObjectType<ContentData>
    {
        private static readonly Func<IResolverContext, object?> Resolver = context =>
        {
            var obj = context.Parent<ContentData>();

            return obj.GetValueOrDefault(context.Field.Name);
        };

        private readonly GraphQLSchemaBuilder builder;
        private readonly ISchemaEntity schema;
        private readonly string schemaName;
        private readonly string schemaType;

        public ContentDataType(GraphQLSchemaBuilder builder, ISchemaEntity schema, string schemaName, string schemaType)
        {
            this.builder = builder;
            this.schema = schema;
            this.schemaName = schemaName;
            this.schemaType = schemaType;
        }

        protected override void Configure(IObjectTypeDescriptor<ContentData> descriptor)
        {
            descriptor.Name($"{schemaType}DataDto")
                .Description($"The structure of the {schemaName} data type.");

            foreach (var (field, fieldName, fieldType) in schema.SchemaDef.Fields.SafeFields())
            {
                if (field.RawProperties is not UIFieldProperties)
                {
                    descriptor.Field(fieldName)
                        .Resolve(Resolver)
                        .Type(new ContentFieldType(builder, schemaName, schemaType, field, fieldName, fieldType))
                        .Description(field.RawProperties.Hints);
                }
            }
        }
    }
}
