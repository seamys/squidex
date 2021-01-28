// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;
using Squidex.Domain.Apps.Entities.Schemas;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents
{
    public sealed record SchemaType(ISchemaEntity Schema, string TypeName, IReadOnlyList<FieldType> Fields)
    {
        public string DisplayName { get; set; } = Schema.DisplayName();

        public string ContentType { get; } = $"{TypeName}Dto";

        public string DataType { get; } = $"{TypeName}DataDto";

        public string DataInputType { get; } = $"{TypeName}DataInputDto";

        public string DataFlatType { get; } = $"{TypeName}FlatDataDto";

        public string ResultType { get; } = $"{TypeName}ResultDto";

        public static SchemaType Build(ISchemaEntity schema)
        {
            var typeName = schema.TypeName();

            var fields =
                schema.SchemaDef.Fields.SafeFields()
                    .Select(x => FieldType.Build(x.Field, x.Name, $"{typeName}{x.Type}"))
                    .ToList();

            return new SchemaType(
                schema,
                schema.TypeName(),
                fields);
        }
    }

    public sealed record FieldType(IField Field, string FieldName, string TypeName, IReadOnlyList<FieldType> Fields)
    {
        private static readonly IReadOnlyList<FieldType> EmptyFields = new List<FieldType>();

        public string DisplayName { get; set; } = Field.DisplayName();

        public string LocalizedType { get; } = $"{TypeName}Dto";

        public string LocalizedInputType { get; } = $"{TypeName}InputDto";

        public string NestedType { get; } = $"{TypeName}ChildDto";

        public string NestedInputType { get; } = $"{TypeName}ChildInputDto";

        public string UnionType { get; } = $"{TypeName}UnionDto";

        public static FieldType Build(IRootField rootField, string fieldName, string typeName)
        {
            var fields = EmptyFields;

            if (rootField is IArrayField arrayField)
            {
                fields =
                    arrayField.Fields.SafeFields()
                        .Select(x => Build(x.Field, x.Name, $"{typeName}{x.Type}"))
                        .ToList();
            }

            return new FieldType(rootField, fieldName, typeName, fields);
        }

        public static FieldType Build(INestedField nestedField, string fieldName, string fieldTypeName)
        {
            return new FieldType(nestedField, fieldName, fieldTypeName, EmptyFields);
        }
    }
}
