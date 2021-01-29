// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.Caching.Memory;

namespace Squidex.Web.GraphQL
{
    public sealed class CachingRequestExecutorResolver : IRequestExecutorResolver
    {
        private readonly IRequestExecutorResolver inner;
        private readonly IMemoryCache memoryCache;

        public event EventHandler<RequestExecutorEvictedEventArgs>? RequestExecutorEvicted
        {
            add
            {
                inner.RequestExecutorEvicted += value;
            }
            remove
            {
                inner.RequestExecutorEvicted -= value;
            }
        }

        public CachingRequestExecutorResolver(IRequestExecutorResolver inner, IMemoryCache memoryCache)
        {
            this.inner = inner;

            this.memoryCache = memoryCache;
        }

        public void EvictRequestExecutor(NameString schemaName = default)
        {
            inner.EvictRequestExecutor(schemaName);
        }

        public async ValueTask<IRequestExecutor> GetRequestExecutorAsync(NameString schemaName = default, CancellationToken cancellationToken = default)
        {
            return await memoryCache.GetOrCreateAsync(schemaName, async x =>
            {
                x.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                x.RegisterPostEvictionCallback(OnCacheEvicted, schemaName);

                var result = await inner.GetRequestExecutorAsync(schemaName, cancellationToken);

                return result;
            });
        }

        private void OnCacheEvicted(object key, object value, EvictionReason reason, object state)
        {
            inner.EvictRequestExecutor((string)state);
        }
    }
}
