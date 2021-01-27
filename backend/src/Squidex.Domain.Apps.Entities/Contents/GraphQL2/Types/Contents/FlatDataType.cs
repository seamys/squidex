// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using HotChocolate.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents
{
    public sealed class FlatDataType : ObjectType<ContentFieldData>
    {
        private readonly GraphQLSchemaBuilder builder;
        private readonly ISchemaEntity schema;
        private readonly string schemaName;
        private readonly string schemaType;

        public FlatDataType(GraphQLSchemaBuilder builder,  ISchemaEntity schema, string schemaName, string schemaType)
        {
            this.builder = builder;
            this.schema = schema;
            this.schemaName = schemaName;
            this.schemaType = schemaType;
        }

        protected override void Configure(IObjectTypeDescriptor<ContentFieldData> descriptor)
        {
            descriptor.Name($"{schemaType}DataFlatDto")
                .Description($"The structure of the flat {schemaName} data type.");

            foreach (var (field, fieldName, _) in schema.SchemaDef.Fields.SafeFields())
            {
                if (field.RawProperties is not UIFieldProperties)
                {
                    var gqlField =
                        descriptor.Field(fieldName)
                            .Description(field.RawProperties.Hints);

                    builder.FieldBuilder.Build(gqlField, field.RawProperties);
                }
            }
        }
    }
}
