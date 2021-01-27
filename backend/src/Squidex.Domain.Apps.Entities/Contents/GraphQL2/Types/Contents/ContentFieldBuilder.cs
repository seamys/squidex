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
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Scalars;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents
{
    public sealed class ContentFieldBuilder
    {
        private readonly GraphQLSchemaBuilder builder;

        private static readonly Func<IResolverContext, object?> NoopResolver = context =>
        {
            var obj = context.Parent<IReadOnlyDictionary<string, IJsonValue>>();

            return obj?.GetValueOrDefault(context.Field.Name);
        };

        public ContentFieldBuilder(GraphQLSchemaBuilder builder)
        {
            this.builder = builder;
        }

        public void Build(IObjectFieldDescriptor field, FieldProperties properties)
        {
            /*
            switch (properties)
            {
                case BooleanFieldProperties:
                    BuildPrimitive(field, NoopType.Boolean, NoopType.NonNullBoolean, properties);
                    break;
                case DateTimeFieldProperties:
                    BuildPrimitive(field, NoopType.DateTime, NoopType.NonNullDateTime, properties);
                    break;
                case NumberFieldProperties:
                    BuildPrimitive(field, NoopType.Float, NoopType.NonNullFloat, properties);
                    break;
                case StringFieldProperties:
                    BuildPrimitive(field, NoopType.String, NoopType.NonNullString, properties);
                    break;
                case TagsFieldProperties:
                    BuildPrimitive(field, NoopType.String, NoopType.NonNullString, properties);
                    break;
            }
            */
        }

        private static void BuildPrimitive(IObjectFieldDescriptor field, IOutputType type, IOutputType nonNullType, FieldProperties properties)
        {
            if (properties.IsRequired)
            {
                field.Type(nonNullType);
            }
            else
            {
                field.Type(type);
            }

            field.Resolve(NoopResolver);
        }
    }
}
