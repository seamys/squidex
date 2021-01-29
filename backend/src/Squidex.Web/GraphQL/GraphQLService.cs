// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Squidex.Infrastructure;
using RequestDelegate = Microsoft.AspNetCore.Http.RequestDelegate;

namespace Squidex.Web.GraphQL
{
    public sealed class GraphQLService
    {
        private readonly IFileProvider fileProvider;
        private readonly IServiceProvider serviceProvider;
        private readonly IRequestExecutorResolver requestExecutorResolver;
        private readonly ConcurrentDictionary<string, Pipeline> pipelines = new ConcurrentDictionary<string, Pipeline>();

        private sealed class Pipeline
        {
            private readonly RequestDelegate requestDeelgate;

            public Pipeline(IServiceProvider serviceProvider, IFileProvider fileProvider, PathString path)
            {
                requestDeelgate =
                    new ApplicationBuilder(serviceProvider)
                        .UseMiddleware<WebSocketSubscriptionMiddleware>(Schema.DefaultName)
                        .UseMiddleware<HttpPostMiddleware>(Schema.DefaultName)
                        .UseMiddleware<HttpGetSchemaMiddleware>(Schema.DefaultName)
                        .UseMiddleware<ToolDefaultFileMiddleware>(fileProvider, path)
                        .UseMiddleware<ToolOptionsFileMiddleware>(Schema.DefaultName, path)
                        .UseMiddleware<ToolStaticFileMiddleware>(fileProvider, path)
                        .UseMiddleware<HttpGetMiddleware>(Schema.DefaultName)
                        .Use(next => context =>
                        {
                            context.Response.StatusCode = 404;

                            return Task.CompletedTask;
                        })
                        .Build();
            }

            public async Task ExecuteAsync(HttpContext httpContext, string pathBase, string path)
            {
                var httpRequest = httpContext.Request;

                var previousPath = httpRequest.Path;
                var previousPathBase = httpRequest.PathBase;
                try
                {
                    httpRequest.PathBase = pathBase;
                    httpRequest.Path = path;

                    await requestDeelgate(httpContext);
                }
                finally
                {
                    httpRequest.PathBase = previousPathBase;
                    httpRequest.Path = previousPath;
                }
            }
        }

        public GraphQLService(IServiceProvider serviceProvider, IRequestExecutorResolver requestExecutorResolver)
        {
            Guard.NotNull(serviceProvider, nameof(serviceProvider));
            Guard.NotNull(requestExecutorResolver, nameof(requestExecutorResolver));

            this.serviceProvider = serviceProvider;

            this.requestExecutorResolver = requestExecutorResolver;
            this.requestExecutorResolver.RequestExecutorEvicted += RequestExecutorResolver_RequestExecutorEvicted;

            fileProvider = CreateFileProvider();
        }

        private void RequestExecutorResolver_RequestExecutorEvicted(object? sender, RequestExecutorEvictedEventArgs e)
        {
            pipelines.TryRemove(e.Name, out _);
        }

        public async Task ExecuteAsync(HttpContext httpContext, string name, string pathBase, string path)
        {
            Guard.NotNull(httpContext, nameof(httpContext));
            Guard.NotNull(name, nameof(name));
            Guard.NotNull(pathBase, nameof(pathBase));
            Guard.NotNull(path, nameof(path));

            var pipeline = pipelines.GetOrAdd(name, x => new Pipeline(serviceProvider, fileProvider, path));

            await pipeline.ExecuteAsync(httpContext, pathBase, path);
        }

        private static IFileProvider CreateFileProvider()
        {
            var resourceNamespace = $"{typeof(MiddlewareBase).Namespace}.Resources";

            return new EmbeddedFileProvider(typeof(MiddlewareBase).Assembly, resourceNamespace);
        }
    }
}
