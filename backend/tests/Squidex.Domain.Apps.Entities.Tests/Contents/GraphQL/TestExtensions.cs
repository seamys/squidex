// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using NodaTime;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public static class TestExtensions
    {
        public static string ToGraphQL(this Instant value)
        {
            return value.ToDateTimeOffset().ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffZ", CultureInfo.InvariantCulture);
        }
    }
}
