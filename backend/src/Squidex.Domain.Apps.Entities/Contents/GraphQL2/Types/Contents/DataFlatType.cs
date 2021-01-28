// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using HotChocolate.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents
{
    public sealed class DataFlatType : ObjectType
    {
        private readonly GraphQLSchemaBuilder builder;
        private readonly SchemaType schemaType;

        public DataFlatType(GraphQLSchemaBuilder builder, SchemaType schemaType)
        {
            this.builder = builder;
            this.schemaType = schemaType;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(schemaType.DataFlatType)
                .Description($"The structure of the {schemaType.DisplayName} flat data type.");

            foreach (var fieldType in schemaType.Fields)
            {
                var field =
                    descriptor.Field(fieldType.FieldName)
                        .WithSourceName(fieldType.Field.Name)
                        .Description(fieldType.Field.RawProperties.Hints);

                builder.FieldBuilder.Build(field, fieldType);
            }
        }
    }
}
