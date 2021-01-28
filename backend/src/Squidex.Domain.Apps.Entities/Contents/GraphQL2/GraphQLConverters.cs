// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics.CodeAnalysis;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types;
using HotChocolate.Utilities;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2
{
    public static class GraphQLConverters
    {
        public static IRequestExecutorBuilder ConfigureTypes(this IRequestExecutorBuilder builder)
        {
            builder.BindRuntimeType<Instant, DateTimeType>();

            builder.AddTypeConverter<DateTimeOffset, Instant>(
                x => Instant.FromDateTimeOffset(x));

            builder.AddTypeConverter<Instant, DateTimeOffset>(
                x => x.ToDateTimeOffset());

            builder.BindRuntimeType<DomainId, StringType>();

            builder.AddTypeConverter<string, DomainId>(
                x => DomainId.Create(x));

            builder.AddTypeConverter<DomainId, string>(
                x => x.ToString());

            builder.BindRuntimeType<RefToken, StringType>();

            builder.AddTypeConverter<string, RefToken>(
                x => RefToken.Parse(x));

            builder.AddTypeConverter<RefToken, string>(
                x => x.ToString());

            builder.AddTypeConverter<TypeConverter>();

            return builder;
        }

        public sealed class TypeConverter : IChangeTypeProvider
        {
            private static readonly ChangeType ConvertJsonNull =
                x => null;
            private static readonly ChangeType ConvertJsonNumber =
                x => ((JsonNumber)x!).Value;
            private static readonly ChangeType ConvertJsonBoolean =
                x => ((JsonBoolean)x!).Value;
            private static readonly ChangeType ConvertJsonString =
                x => ((JsonString)x!).Value;
            private static readonly ChangeType ConvertJsonStringToDateTimeOffset =
                x => DateTimeOffset.Parse(((JsonString)x!).Value);

            public bool TryCreateConverter(Type source, Type target, ChangeTypeProvider root, [NotNullWhen(true)] out ChangeType? converter)
            {
                if (source == typeof(JsonString))
                {
                    if (target == typeof(DateTimeOffset))
                    {
                        converter = ConvertJsonStringToDateTimeOffset;
                    }
                    else
                    {
                        converter = ConvertJsonString;
                    }

                    return true;
                }

                if (source == typeof(JsonBoolean))
                {
                    converter = ConvertJsonBoolean;
                    return true;
                }

                if (source == typeof(JsonNumber))
                {
                    converter = ConvertJsonNumber;
                    return true;
                }

                if (source == typeof(JsonNull))
                {
                    converter = ConvertJsonNull;
                    return true;
                }

                converter = null;
                return false;
            }
        }
    }
}
