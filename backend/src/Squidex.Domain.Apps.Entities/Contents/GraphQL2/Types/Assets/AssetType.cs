// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Scalars;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Assets
{
    internal class AssetType : ObjectType<IEnrichedAssetEntity>
    {
        private readonly IUrlGenerator urlGenerator;

        public AssetType(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        protected override void Configure(IObjectTypeDescriptor<IEnrichedAssetEntity> descriptor)
        {
            descriptor.Name("AssetDto")
                .Description("An asset.");

            descriptor.Field(x => x.Id)
                .Type<NonNullType<StringType>>()
                .Description("The id of the asset.");

            descriptor.Field(x => x.Version)
               .Type<NonNullType<LongType>>()
               .Description("The version of the asset.");

            descriptor.Field(x => x.Created)
                .Type<NonNullType<DateTimeType>>()
                .Description("The date and time when the asset has been created.");

            descriptor.Field(x => x.CreatedBy)
                .Type<NonNullType<StringType>>()
                .Description("The user that has created the asset.");

            descriptor.Field(x => x.LastModified)
                .Type<NonNullType<DateTimeType>>()
                .Description("The date and time when the asset has been modified last.");

            descriptor.Field(x => x.LastModifiedBy)
                .Type<NonNullType<StringType>>()
                .Description("The user that has updated the asset last.");

            descriptor.Field(x => x.MimeType)
                .Type<NonNullType<StringType>>()
                .Description("The mime type.");

            descriptor.Field(x => x.FileName)
                .Type<NonNullType<StringType>>()
                .Description("The file name.");

            descriptor.Field(x => x.FileVersion)
               .Type<NonNullType<LongType>>()
               .Description("The file version of the asset.");

            descriptor.Field(x => x.FileSize)
               .Type<NonNullType<LongType>>()
               .Description("The size of the file in bytes.");

            descriptor.Field(x => x.FileHash)
                .Type<NonNullType<StringType>>()
                .Description("The hash of the file. Can be null for old files.");

            descriptor.Field(x => x.Slug)
                .Type<NonNullType<StringType>>()
                .Description("The file name as slug.");

            descriptor.Field(x => x.IsProtected)
                .Type<NonNullType<BooleanType>>()
                .Description("True, when the asset is not public.");

            descriptor.Field(x => x.Type)
                .Type<NonNullType<EnumType<Core.Assets.AssetType>>>()
                .Description("The type of the image.");

            descriptor.Field(x => x.MetadataText)
                .Type<NonNullType<StringType>>()
                .Description("The text representation of the metadata.");

            descriptor.Field("tags").Resolve(Resolver(x => x.TagNames))
                .Type<ListType<NonNullType<StringType>>>()
                .Description("The asset tags.");

            descriptor.Field("fileType").Resolve(Resolver(x => x.FileName.FileType()))
                .Type<NonNullType<StringType>>()
                .Description("The text representation of the metadata.");

            descriptor.Field("url").Resolve(Url)
                .Type<NonNullType<StringType>>()
                .Description("The url to the asset.");

            descriptor.Field("thumbnailUrl").Resolve(ThumbnailUrl)
                .Type<NonNullType<StringType>>()
                .Description("The thumbnail url to the asset.");

            descriptor.Field("isImage").Resolve(Resolver(x => x.Type == Core.Assets.AssetType.Image))
                .Type<NonNullType<BooleanType>>()
                .Description("Determines if the uploaded file is an image.");

            descriptor.Field("pixelHeight").Resolve(Resolver(x => x.Metadata.GetPixelHeight()))
                .Type<IntType>()
                .Description("The height of the image in pixels if the asset is an image.");

            descriptor.Field("pixelWidth").Resolve(Resolver(x => x.Metadata.GetPixelWidth()))
                .Type<IntType>()
                .Description("The width of the image in pixels if the asset is an image.");

            descriptor.Field("metadata").Resolve(Metadata)
                .Type<AnyJsonType>().Argument("path", arg => arg
                    .Type<StringType>()
                    .Description("The optional path in the metadata object."))
                .Description("The asset metadata.");

            if (urlGenerator.CanGenerateAssetSourceUrl)
            {
                descriptor.Field("sourceUrl").Resolve(SourceUrl)
                    .Type<NonNullType<StringType>>()
                    .Description("The source url to the asset.");
            }
        }

        private static readonly Func<IResolverContext, string> Url = Resolver((asset, context) =>
        {
            var urlGenerator = context.Service<IUrlGenerator>();

            return urlGenerator.AssetContent(asset.AppId, asset.Id.ToString());
        });

        private static readonly Func<IResolverContext, string?> SourceUrl = Resolver((asset, context) =>
        {
            var urlGenerator = context.Service<IUrlGenerator>();

            return urlGenerator.AssetSource(asset.AppId, asset.Id, asset.FileVersion);
        });

        private static readonly Func<IResolverContext, string?> ThumbnailUrl = Resolver((asset, context) =>
        {
            var urlGenerator = context.Service<IUrlGenerator>();

            return urlGenerator.AssetThumbnail(asset.AppId, asset.Id.ToString(), asset.Type);
        });

        private static readonly Func<IResolverContext, object?> Metadata = Resolver((asset, context) =>
        {
            var path = context.ArgumentValue<string?>("path");

            if (!string.IsNullOrWhiteSpace(path))
            {
                asset.Metadata.TryGetByPath(path, out var result);

                return result;
            }

            return asset.Metadata;
        });

        private static Func<IResolverContext, T> Resolver<T>(Func<IEnrichedAssetEntity, T> resolver)
        {
            return context => resolver(context.Parent<IEnrichedAssetEntity>());
        }

        private static Func<IResolverContext, T> Resolver<T>(Func<IEnrichedAssetEntity, IResolverContext, T> resolver)
        {
            return context => resolver(context.Parent<IEnrichedAssetEntity>(), context);
        }
    }
}
