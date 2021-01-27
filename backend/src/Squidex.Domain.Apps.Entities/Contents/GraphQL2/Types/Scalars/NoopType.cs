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
    public sealed class NoopType : ScalarType
    {
        /*
        public static readonly IOutputType Any = new NoopType(new AnyType());
        public static readonly IOutputType NonNullAny = new NonNullType(new NoopType(new AnyType()));

        public static readonly IOutputType Boolean = new NoopType(new BooleanType());
        public static readonly IOutputType NonNullBoolean = new NonNullType(new NoopType(new BooleanType()));

        public static readonly IOutputType DateTime = new NoopType(new DateTimeType());
        public static readonly IOutputType NonNullDateTime = new NonNullType(new NoopType(new DateTimeType()));

        public static readonly IOutputType Float = new NoopType(new FloatType());
        public static readonly IOutputType NonNullFloat = new NonNullType(new NoopType(new FloatType()));

        public static readonly IOutputType String = new NoopType(new StringType());
        public static readonly IOutputType NonNullString = new NonNullType(new NoopType(new StringType()));
        */

        public override Type RuntimeType => typeof(object);

        public NoopType(TypeSystemObjectBase type)
            : base(type.Name)
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
}
