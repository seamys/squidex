// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents
{
    public sealed class DataType : ObjectType
    {
        private readonly SchemaType schemaType;

        public DataType(SchemaType schemaType)
        {
            this.schemaType = schemaType;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(schemaType.DataType)
                .Description($"The structure of the {schemaType.DisplayName} data type.");

            foreach (var fieldType in schemaType.Fields)
            {
                var field =
                    descriptor.Field(fieldType.FieldName)
                        .WithSourceName(fieldType.Field.Name)
                        .Resolve(Resolver)
                        .Description(fieldType.Field.RawProperties.Hints);

                if (fieldType.Field.RawProperties.IsRequired)
                {
                    field.Type(new NonNullTypeNode(new NamedTypeNode(fieldType.LocalizedType)));
                }
                else
                {
                    field.Type(new NamedTypeNode(fieldType.LocalizedType));
                }
            }
        }

        private static readonly FieldResolverDelegate Resolver = context =>
        {
            var obj = context.Parent<ContentData>();

            var result = obj.GetValueOrDefault(context.SourceName());

            return new ValueTask<object?>(result);
        };
    }
}
