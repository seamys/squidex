// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types
{
    public sealed class GraphQLSchemaBuilder
    {
        private readonly Dictionary<FieldType, string> unionTypes = new Dictionary<FieldType, string>();
#pragma warning disable IDE0044 // Add readonly modifier
        private PartitionResolver partitionResolver;
#pragma warning restore IDE0044 // Add readonly modifier

        public ISchemaBuilder? CurrentBuilder { get; private set; }

        public IReadOnlyList<SchemaType> SchemaTypes { get; }

        public ContentFieldBuilder FieldBuilder { get; }

        public GraphQLSchemaBuilder(IAppEntity app, IEnumerable<ISchemaEntity> schemas)
        {
            partitionResolver = app.PartitionResolver();

            FieldBuilder = new ContentFieldBuilder(this);

            SchemaTypes = schemas.Select(SchemaType.Build).ToList();
        }

        internal IFieldPartitioning ResolvePartition(Partitioning key)
        {
            return partitionResolver(key);
        }

        public void BuildSchema(ISchemaBuilder builder)
        {
            CurrentBuilder = builder;

            builder.AddType(new ReferencesUnionType("AllContents", SchemaTypes));

            foreach (var schemaType in SchemaTypes)
            {
                foreach (var fieldType in schemaType.Fields)
                {
                    builder.AddType(new ContentFieldType(this, schemaType, fieldType));

                    BuildUnionType(builder, fieldType);

                    if (fieldType.Fields.Count > 0)
                    {
                        builder.AddType(new NestedType(this, schemaType, fieldType));
                    }

                    foreach (var nestedType in fieldType.Fields)
                    {
                        BuildUnionType(builder, nestedType);
                    }
                }

                builder.AddType(new ContentType(schemaType));
                builder.AddType(new ContentResultType(schemaType));
                builder.AddType(new DataType(schemaType));
                builder.AddType(new DataFlatType(this, schemaType));
            }

            builder.AddQueryType(new QueryType(SchemaTypes));

            CurrentBuilder = null;
        }

        public string GetUnionTypeName(FieldType fieldType)
        {
            return unionTypes[fieldType];
        }

        private void BuildUnionType(ISchemaBuilder builder, FieldType fieldType)
        {
            if (fieldType.Field.RawProperties is ReferencesFieldProperties references)
            {
                var referenced =
                    references.SchemaIds.OrEmpty()
                        .Select(x => SchemaTypes.FirstOrDefault(y => y.Schema.Id == x)).NotNull()
                        .ToList();

                if (referenced.Count == 1)
                {
                    unionTypes[fieldType] = referenced[0].ContentType;
                }
                else if (referenced.Count == SchemaTypes.Count || referenced.Count == 0)
                {
                    var typeName = "AllSchemasUnionDto";

                    if (!unionTypes.Values.Contains(typeName))
                    {
                        builder.AddType(new ReferencesUnionType(typeName, SchemaTypes));
                    }

                    unionTypes[fieldType] = typeName;
                }
                else
                {
                    builder.AddType(new ReferencesUnionType(fieldType.UnionType, referenced));

                    unionTypes[fieldType] = fieldType.UnionType;
                }
            }
        }
    }
}
