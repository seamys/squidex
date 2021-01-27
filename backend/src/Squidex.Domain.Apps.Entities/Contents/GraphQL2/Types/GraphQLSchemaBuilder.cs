// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using HotChocolate;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types
{
    public sealed class GraphQLSchemaBuilder
    {
        private readonly Dictionary<DomainId, ContentType> contentTypes = new Dictionary<DomainId, ContentType>();
        private readonly IEnumerable<ISchemaEntity> schemas;
#pragma warning disable IDE0044 // Add readonly modifier
        private PartitionResolver partitionResolver;
#pragma warning restore IDE0044 // Add readonly modifier

        internal ContentFieldBuilder FieldBuilder { get; }

        public GraphQLSchemaBuilder(IAppEntity app, IEnumerable<ISchemaEntity> schemas)
        {
            partitionResolver = app.PartitionResolver();

            FieldBuilder = new ContentFieldBuilder(this);

            foreach (var schema in schemas)
            {
                // contentTypes[schema.Id] = new ContentType(this, schema);
            }

            this.schemas = schemas;
        }

        internal ContentType GetContentType(DomainId id)
        {
            return contentTypes[id];
        }

        internal IFieldPartitioning ResolvePartition(Partitioning key)
        {
            return partitionResolver(key);
        }

        public void BuildSchema(ISchemaBuilder builder)
        {
            builder.AddQueryType<QueryType>();
        }
    }
}
