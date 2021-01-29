// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.Execution.Configuration;
using HotChocolate.Execution.Options;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents.GraphQL2.Types;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Log;

namespace Squidex.Web.GraphQL
{
    public sealed class AppRequestExecutorOptionsMonitor : IRequestExecutorOptionsMonitor
    {
        public ValueTask<RequestExecutorSetup> GetAsync(NameString schemaName, CancellationToken cancellationToken)
        {
            var setup = new RequestExecutorSetup();

            setup.SchemaBuilderActions.Add(new SchemaBuilderAction(OnSetupSchemaAsync));

            setup.SchemaServices.Add(services =>
            {
                services.AddSingleton<IHttpRequestInterceptor, ContextInterceptor>();
            });

            return new ValueTask<RequestExecutorSetup>(setup);
        }

        private async ValueTask OnSetupSchemaAsync(IServiceProvider services, ISchemaBuilder builder, CancellationToken ct)
        {
            var app = services.GetRequiredService<IContextProvider>().Context.App;

            var schemas = await GetSchemasAsync(services, app);

            new GraphQLSchemaBuilder(app, schemas)
                .BuildSchema(builder);
        }

        private static async Task<List<ISchemaEntity>> GetSchemasAsync(IServiceProvider services, Domain.Apps.Entities.Apps.IAppEntity app)
        {
            var appProvider = services.GetRequiredService<IAppProvider>();

            return await appProvider.GetSchemasAsync(app.Id);
        }

        public IDisposable OnChange(Action<NameString> listener)
        {
            return NoopDisposable.Instance;
        }
    }
}
