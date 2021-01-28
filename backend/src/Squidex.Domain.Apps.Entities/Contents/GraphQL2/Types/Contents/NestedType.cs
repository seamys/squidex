// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using HotChocolate.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents
{
    public sealed class NestedType : ObjectType
    {
        private readonly GraphQLSchemaBuilder builder;
        private readonly SchemaType schemaType;
        private readonly FieldType fieldType;

        public NestedType(GraphQLSchemaBuilder builder, SchemaType schemaType, FieldType fieldType)
        {
            this.builder = builder;
            this.schemaType = schemaType;
            this.fieldType = fieldType;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(fieldType.NestedType)
                .Description($"The structure of the {schemaType.DisplayName}/{fieldType} nested schema");

            foreach (var nestedField in fieldType.Fields)
            {
                var field =
                    descriptor.Field(nestedField.FieldName)
                        .WithSourceName(nestedField.Field.Name)
                        .Description(nestedField.Field.RawProperties.Hints);

                builder.FieldBuilder.Build(field, nestedField);
            }
        }
    }
}
