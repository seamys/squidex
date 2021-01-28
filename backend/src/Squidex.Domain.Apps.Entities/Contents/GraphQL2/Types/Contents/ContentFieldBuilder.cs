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
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Scalars;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents
{
    public sealed class ContentFieldBuilder
    {
        private static readonly IReadOnlyList<IEnrichedAssetEntity> EmptyAssets = new List<IEnrichedAssetEntity>();
        private static readonly IReadOnlyList<IEnrichedContentEntity> EmptyContents = new List<IEnrichedContentEntity>();
        private readonly GraphQLSchemaBuilder builder;

        public ContentFieldBuilder(GraphQLSchemaBuilder builder)
        {
            this.builder = builder;
        }

        public void Build(IObjectFieldDescriptor descriptor, FieldType fieldType)
        {
            var properties = fieldType.Field.RawProperties;

            switch (properties)
            {
                case ArrayFieldProperties:
                    BuildArray(descriptor, fieldType, properties);
                    break;
                case ReferencesFieldProperties r:
                    BuildReferences(descriptor, fieldType, r);
                    break;
                case AssetsFieldProperties:
                    BuildAssets(descriptor, properties);
                    break;
                case BooleanFieldProperties:
                    BuildPrimitive<BooleanType>(descriptor, properties);
                    break;
                case DateTimeFieldProperties:
                    BuildPrimitive<DateTimeType>(descriptor, properties);
                    break;
                case GeolocationFieldProperties:
                    BuildPrimitive<AnyJsonType>(descriptor, properties);
                    break;
                case JsonFieldProperties:
                    BuildJson(descriptor, properties);
                    break;
                case NumberFieldProperties:
                    BuildPrimitive<FloatType>(descriptor, properties);
                    break;
                case StringFieldProperties:
                    BuildPrimitive<StringType>(descriptor, properties);
                    break;
                case TagsFieldProperties:
                    BuildPrimitive<ListType<NonNullType<StringType>>>(descriptor, properties);
                    break;
            }
        }

        private static void BuildAssets(IObjectFieldDescriptor field, FieldProperties properties)
        {
            var type = new ListTypeNode(new NonNullTypeNode(new NamedTypeNode("AssetDto")));

            if (properties.IsRequired)
            {
                field.Type(new NonNullTypeNode(type));
            }
            else
            {
                field.Type(type);
            }

            field.Resolve(AssetsResolver);
        }

        private void BuildReferences(IObjectFieldDescriptor field, FieldType fieldType, ReferencesFieldProperties properties)
        {
            var type = new ListTypeNode(new NonNullTypeNode(new NamedTypeNode(builder.GetUnionTypeName(fieldType))));

            if (properties.IsRequired)
            {
                field.Type(new NonNullTypeNode(type));
            }
            else
            {
                field.Type(type);
            }

            field.Resolve(ReferencesResolver);
        }

        private static void BuildArray(IObjectFieldDescriptor field, FieldType fieldType, FieldProperties properties)
        {
            var type = new ListTypeNode(new NonNullTypeNode(new NamedTypeNode(fieldType.NestedType)));

            if (properties.IsRequired)
            {
                field.Type(new NonNullTypeNode(type));
            }
            else
            {
                field.Type(type);
            }

            field.Resolve(NoopResolver);
        }

        private static void BuildPrimitive<T>(IObjectFieldDescriptor field, FieldProperties properties) where T : class, IOutputType
        {
            if (properties.IsRequired)
            {
                field.Type<NonNullType<T>>();
            }
            else
            {
                field.Type<T>();
            }

            field.Resolve(NoopResolver);
        }

        private static void BuildJson(IObjectFieldDescriptor field, FieldProperties properties)
        {
            if (properties.IsRequired)
            {
                field.Type<NonNullType<AnyJsonType>>();
            }
            else
            {
                field.Type<AnyJsonType>();
            }

            field.Argument("path", arg => arg
                .Type<StringType>()
                .Description("The path in the json object."));

            field.Resolve(JsonResolver);
        }

        private static readonly FieldResolverDelegate AssetsResolver = async context =>
        {
            var obj = context.Parent<IReadOnlyDictionary<string, IJsonValue?>>();

            var result = EmptyAssets;

            if (obj.TryGetValue(context.SourceName(), out var json))
            {
                var ids = ParseIds(json);

                if (ids != null)
                {
                    result = await context.AssetDataLoader().LoadAsync(ids, context.RequestAborted);
                }
            }

            return result;
        };

        private static readonly FieldResolverDelegate ReferencesResolver = async context =>
        {
            var obj = context.Parent<IReadOnlyDictionary<string, IJsonValue?>>();

            var result = EmptyContents;

            if (obj.TryGetValue(context.SourceName(), out var json))
            {
                var ids = ParseIds(json);

                if (ids != null)
                {
                    result = await context.ContentDataLoader().LoadAsync(ids, context.RequestAborted);
                }
            }

            return result;
        };

        private static readonly FieldResolverDelegate JsonResolver = context =>
        {
            var obj = context.Parent<IReadOnlyDictionary<string, IJsonValue?>>();

            if (obj.TryGetValue(context.SourceName(), out var json) && json != null)
            {
                var path = context.ArgumentValue<string?>("path");

                if (!string.IsNullOrWhiteSpace(path))
                {
                    json.TryGetByPath(path, out var temp);
                    json = temp;
                }
            }

            return new ValueTask<object?>(json is JsonNull ? null : json);
        };

        private static readonly FieldResolverDelegate NoopResolver = context =>
        {
            var obj = context.Parent<IReadOnlyDictionary<string, IJsonValue?>>();

            var result = obj.GetValueOrDefault(context.SourceName());

            return new ValueTask<object?>(result is JsonNull ? null : result);
        };

        private static List<DomainId>? ParseIds(IJsonValue? value)
        {
            if (value == null)
            {
                return null;
            }

            try
            {
                var result = new List<DomainId>();

                if (value is JsonArray array)
                {
                    foreach (var id in array)
                    {
                        result.Add(DomainId.Create(id.ToString()));
                    }
                }

                if (result.Count == 0)
                {
                    return null;
                }

                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}
