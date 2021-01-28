// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using HotChocolate.Language;
using HotChocolate.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Contents
{
    internal sealed class ReferencesUnionType : UnionType
    {
        private readonly string name;
        private readonly IEnumerable<SchemaType> schemaTypes;

        public ReferencesUnionType(string name, IEnumerable<SchemaType> schemaTypes)
        {
            this.name = name;
            this.schemaTypes = schemaTypes;
            this.schemaTypes = schemaTypes;
        }

        protected override void Configure(IUnionTypeDescriptor descriptor)
        {
            descriptor.Name(name);

            foreach (var type in schemaTypes)
            {
                descriptor.Type(new NamedTypeNode(type.ContentType));
            }
        }
    }
}
