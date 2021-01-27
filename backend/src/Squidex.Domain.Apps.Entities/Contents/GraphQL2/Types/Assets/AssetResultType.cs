// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Assets
{
    public class AssetResultType : ObjectType<IResultList<IEnrichedAssetEntity>>
    {
        protected override void Configure(IObjectTypeDescriptor<IResultList<IEnrichedAssetEntity>> descriptor)
        {
            descriptor.Name("AssetResultDto")
                .Description("List of assets and total count of assets.");

            descriptor.Field(x => x.Total)
                .Type<NonNullType<LongType>>()
                .Description("The total count of assets.");

            descriptor.Field("items").Resolve(Resolver(x => x))
                .Type<ListType<NonNullType<AssetType>>>()
                .Description("The assets.");
        }

        private static Func<IResolverContext, T> Resolver<T>(Func<IResultList<IEnrichedAssetEntity>, T> resolver)
        {
            return context => resolver(context.Parent<IResultList<IEnrichedAssetEntity>>());
        }
    }
}
