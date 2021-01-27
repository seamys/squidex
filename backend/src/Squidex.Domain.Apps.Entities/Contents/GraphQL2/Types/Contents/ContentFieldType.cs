// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using HotChocolate.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents
{
    internal sealed class ContentFieldType : ObjectType<ContentFieldData>
    {
        private readonly GraphQLSchemaBuilder builder;
        private readonly string schemaName;
        private readonly string schemaType;
        private readonly IRootField field;
        private readonly string fieldName;
        private readonly string fieldType;

        public ContentFieldType(GraphQLSchemaBuilder builder,
            string schemaName,
            string schemaType,
            IRootField field,
            string fieldName,
            string fieldType)
        {
            this.builder = builder;
            this.schemaName = schemaName;
            this.schemaType = schemaType;
            this.field = field;
            this.fieldName = fieldName;
            this.fieldType = fieldType;
        }

        protected override void Configure(IObjectTypeDescriptor<ContentFieldData> descriptor)
        {
            descriptor.Name($"{schemaType}Data{fieldType}Dto")
                .Description($"The structure of the {field.DisplayName()} (${fieldName}) field of the {schemaName} content type.");

            var partition = builder.ResolvePartition(field.Partitioning);

            foreach (var key in partition.AllKeys)
            {
                var gqlField =
                    descriptor.Field(key)
                        .Description(field.RawProperties.Hints);

                builder.FieldBuilder.Build(gqlField, field.RawProperties);
            }
        }
    }
}
