// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using HotChocolate.Language;
using HotChocolate.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types.Scalars
{
    public abstract class NoopType : ScalarType
    {
        public override Type RuntimeType => typeof(object);

        protected NoopType(string name)
            : base(name)
        {
        }

        public override bool IsInstanceOfType(IValueNode valueSyntax)
        {
            throw new NotSupportedException();
        }

        public override object? ParseLiteral(IValueNode valueSyntax, bool withDefaults = true)
        {
            throw new NotSupportedException();
        }

        public override IValueNode ParseResult(object? resultValue)
        {
            throw new NotSupportedException();
        }

        public override IValueNode ParseValue(object? runtimeValue)
        {
            throw new NotSupportedException();
        }

        public override bool TryDeserialize(object? resultValue, out object? runtimeValue)
        {
            runtimeValue = resultValue;
            return true;
        }

        public override bool TrySerialize(object? runtimeValue, out object? resultValue)
        {
            resultValue = runtimeValue;
            return true;
        }
    }

    public sealed class NoopDateTime : NoopType
    {
        public NoopDateTime()
            : base("DateTime")
        {
        }
    }
}
