// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents
{
    internal sealed class ContentResultType : ObjectType
    {
        private readonly SchemaType schemaType;

        public ContentResultType(SchemaType schemaType)
        {
            this.schemaType = schemaType;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(schemaType.ResultType)
                .Description($"List of {schemaType.DisplayName} items and total count.");

            descriptor.Field("total").Resolve(Resolver(x => x.Total))
                .Type<NonNullType<LongType>>()
                .Description("The total count of assets.");

            descriptor.Field("items").Resolve(Resolver(x => x))
                .Type(new NonNullTypeNode(new ListTypeNode(new NonNullTypeNode(new NamedTypeNode(schemaType.ContentType)))))
                .Description("The contents.");
        }

        private static FieldResolverDelegate Resolver<T>(Func<IResultList<IEnrichedContentEntity>, T> resolver)
        {
            return context => new ValueTask<object?>(resolver(context.Parent<IResultList<IEnrichedContentEntity>>()));
        }
    }
}
