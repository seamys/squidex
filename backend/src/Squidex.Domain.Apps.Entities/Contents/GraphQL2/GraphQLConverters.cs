// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Squidex.Infrastructure;

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

            return builder;
        }
    }
}
