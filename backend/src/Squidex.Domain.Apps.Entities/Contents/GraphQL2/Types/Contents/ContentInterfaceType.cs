// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using HotChocolate.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents
{
    public class ContentInterfaceType : InterfaceType
    {
        protected override void Configure(IInterfaceTypeDescriptor descriptor)
        {
            descriptor.Name("Content")
                .Description("The basic structure for all content types.");

            descriptor.Field("id")
                .Type<NonNullType<StringType>>()
                .Description("The id of the content.");

            descriptor.Field("version")
               .Type<NonNullType<LongType>>()
               .Description("The version of the content.");

            descriptor.Field("created")
                .Type<NonNullType<DateTimeType>>()
                .Description("The date and time when the content has been created.");

            descriptor.Field("createdBy")
                .Type<NonNullType<StringType>>()
                .Description("The user that has created the content.");

            descriptor.Field("lastModified")
                .Type<NonNullType<DateTimeType>>()
                .Description("The date and time when the content has been modified last.");

            descriptor.Field("lastModifiedBy")
                .Type<NonNullType<StringType>>()
                .Description("The user that has updated the content last.");

            descriptor.Field("status")
                .Type<NonNullType<StringType>>()
                .Description("The status.");

            descriptor.Field("statusColor")
                .Type<NonNullType<StringType>>()
                .Description("The status color.");

            descriptor.Field("url")
                .Type<NonNullType<StringType>>()
                .Description("The url to the content.");
        }
    }
}
