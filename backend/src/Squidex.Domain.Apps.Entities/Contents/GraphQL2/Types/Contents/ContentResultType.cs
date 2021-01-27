// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents
{
    internal sealed class ContentResultType : ObjectType<IResultList<IEnrichedContentEntity>>
    {
        private readonly string schemaType;
        private readonly string schemaName;
        private readonly IType contentType;

        public ContentResultType(string schemaType, string schemaName, IType contentType)
        {
            this.schemaType = schemaType;
            this.schemaName = schemaName;
            this.contentType = contentType;
        }

        protected override void Configure(IObjectTypeDescriptor<IResultList<IEnrichedContentEntity>> descriptor)
        {
            descriptor.Name($"{schemaType}ResultDto")
                .Description($"List of {schemaName} items and total count.");

            descriptor.Field(x => x.Total)
                .Type<NonNullType<LongType>>()
                .Description("The total count of assets.");

            descriptor.Field("items").Resolve(Resolver(x => x))
                .Type(new ListType(new NonNullType(contentType)))
                .Description("The contents.");
        }

        private static Func<IResolverContext, T> Resolver<T>(Func<IResultList<IEnrichedContentEntity>, T> resolver)
        {
            return context => resolver(context.Parent<IResultList<IEnrichedContentEntity>>());
        }
    }
}
