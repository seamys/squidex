// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using IField = Squidex.Domain.Apps.Core.Schemas.IField;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents
{
    internal sealed class ContentType : ObjectType
    {
        private readonly GraphQLSchemaBuilder builder;
        private readonly SchemaType schemaType;

        public ContentType(GraphQLSchemaBuilder builder, SchemaType schemaType)
        {
            this.builder = builder;
            this.schemaType = schemaType;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(schemaType.ContentType)
                .Description($"The structure of a {schemaType.DisplayName} content type.");

            var schemaId = schemaType.Schema.Id;

            descriptor.IsOfType((context, result) =>
            {
                return result is IContentEntity content && content.SchemaId.Id == schemaId;
            });

            descriptor.Field("id").Resolve(Id)
                .Type<NonNullType<StringType>>()
                .Description("The id of the content.");

            descriptor.Field("version").Resolve(Version)
               .Type<NonNullType<LongType>>()
               .Description("The version of the content.");

            descriptor.Field("created").Resolve(Created)
                .Type<NonNullType<DateTimeType>>()
                .Description("The date and time when the content has been created.");

            descriptor.Field("createdBy").Resolve(CreatedBy)
                .Type<NonNullType<StringType>>()
                .Description("The user that has created the content.");

            descriptor.Field("lastModified").Resolve(LastModified)
                .Type<NonNullType<DateTimeType>>()
                .Description("The date and time when the content has been modified last.");

            descriptor.Field("lastModifiedBy").Resolve(LastModifiedBy)
                .Type<NonNullType<StringType>>()
                .Description("The user that has updated the content last.");

            descriptor.Field("status").Resolve(Status)
                .Type<NonNullType<StringType>>()
                .Description("The status.");

            descriptor.Field("statusColor").Resolve(StatusColor)
                .Type<NonNullType<StringType>>()
                .Description("The status color.");

            descriptor.Field("url").Resolve(Url)
                .Type<NonNullType<StringType>>()
                .Description("The url to the content.");

            descriptor.Field("data").Resolve(Data)
                .Type(new NonNullTypeNode(new NamedTypeNode(schemaType.DataType)))
                .Description("The content data.");

            descriptor.Field("flatData").Resolve(FlatData)
                .Type(new NonNullTypeNode(new NamedTypeNode(schemaType.DataFlatType)))
                .Description("The flat content data.");

            descriptor.Interface(new NamedTypeNode("Content"));

            foreach (var other in builder.SchemaTypes.Where(x => IsReferencing(x, schemaType)))
            {
                descriptor.Field($"referencing{other.TypeName}Contents").Resolve(QueryReferencing)
                    .Type(new NonNullTypeNode(new ListTypeNode(new NonNullTypeNode(new NamedTypeNode(other.ContentType)))))
                    .UseSchemaId(other.Schema.Id)
                    .UseODataArgs()
                    .Description($"Query {other.TypeName} content items");

                descriptor.Field($"referencing{other.TypeName}ContentsWithTotal").Resolve(QueryReferencing)
                    .Type(new NonNullTypeNode(new NamedTypeNode(other.ResultType)))
                    .UseSchemaId(other.Schema.Id)
                    .UseODataArgs()
                    .Description($"Query {other.TypeName} content items with total count.");
            }
        }

        private static bool IsReferencing(SchemaType other, SchemaType schema)
        {
            return other.Schema.SchemaDef.Fields.Any(x => IsReferencing(x, schema.Schema.Id));
        }

        private static bool IsReferencing(IField field, DomainId id)
        {
            switch (field)
            {
                case IField<ReferencesFieldProperties> reference:
                    return
                        reference.Properties.SchemaIds == null ||
                        reference.Properties.SchemaIds.Count == 0 ||
                        reference.Properties.SchemaIds.Contains(id);
                case IArrayField arrayField:
                    return arrayField.Fields.Any(x => IsReferencing(x, id));
            }

            return false;
        }

        private static readonly FieldResolverDelegate Id = Resolver(content => content.Id);
        private static readonly FieldResolverDelegate Version = Resolver(content => content.Version);
        private static readonly FieldResolverDelegate CreatedBy = Resolver(content => content.CreatedBy);
        private static readonly FieldResolverDelegate Created = Resolver(content => content.Created);
        private static readonly FieldResolverDelegate LastModified = Resolver(content => content.LastModified);
        private static readonly FieldResolverDelegate LastModifiedBy = Resolver(content => content.LastModifiedBy);
        private static readonly FieldResolverDelegate Status = Resolver(content => content.Status.ToString().ToUpperInvariant());
        private static readonly FieldResolverDelegate StatusColor = Resolver(content => content.StatusColor);
        private static readonly FieldResolverDelegate Data = Resolver(content => content.Data);

        private static readonly FieldResolverDelegate Url = Resolver((content, context) =>
        {
            var appId = content.AppId;

            return context.Service<IUrlGenerator>().ContentUI(appId, content.SchemaId, content.Id);
        });

        private static readonly FieldResolverDelegate FlatData = Resolver((content, context) =>
        {
            var masterLanguage = context.RequestContext().App.Languages.Master;

            return content.Data.ToFlatten(masterLanguage);
        });

        private static FieldResolverDelegate Resolver<T>(Func<IEnrichedContentEntity, IResolverContext, T> resolver)
        {
            return context => new ValueTask<object?>(resolver(context.Parent<IEnrichedContentEntity>(), context));
        }

        private static FieldResolverDelegate Resolver<T>(Func<IEnrichedContentEntity, T> resolver)
        {
            return context => new ValueTask<object?>(resolver(context.Parent<IEnrichedContentEntity>()));
        }

        private static readonly FieldResolverDelegate QueryReferencing = async context =>
        {
            var contentQuery = context.Service<IContentQueryService>();

            var q = Q.Empty.WithODataQuery(context.BuildODataQuery()).WithReference(context.Parent<IEnrichedContentEntity>().Id);

            return await contentQuery.QueryAsync(context.RequestContext(), context.SchemaId(), q);
        };
    }
}
