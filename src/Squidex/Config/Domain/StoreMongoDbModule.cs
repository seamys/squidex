﻿// ==========================================================================
//  StoreMongoDbModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using Autofac.Core;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.CQRS.Replay;
using Squidex.Read.Apps.Repositories;
using Squidex.Read.Contents.Repositories;
using Squidex.Read.History.Repositories;
using Squidex.Read.MongoDb;
using Squidex.Read.MongoDb.Apps;
using Squidex.Read.MongoDb.Contents;
using Squidex.Read.MongoDb.History;
using Squidex.Read.MongoDb.Infrastructure;
using Squidex.Read.MongoDb.Schemas;
using Squidex.Read.MongoDb.Users;
using Squidex.Read.Schemas.Repositories;
using Squidex.Read.Users.Repositories;

namespace Squidex.Config.Domain
{
    public class StoreMongoDbModule : Module
    {
        private const string MongoDatabaseName = "string";

        public IConfiguration Configuration { get; }

        public StoreMongoDbModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var databaseName = Configuration.GetValue<string>("squidex:stores:mongoDb:databaseName");

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new ConfigurationException("You must specify the MongoDB database name in the 'squidex:stores:mongoDb:databaseName' configuration section.");
            }

            var connectionString = Configuration.GetValue<string>("squidex:stores:mongoDb:connectionString");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ConfigurationException("You must specify the MongoDB connection string in the 'squidex:stores:mongoDb:connectionString' configuration section.");
            }

            builder.Register(context =>
            {
                var mongoDbClient = new MongoClient(connectionString);
                var mongoDatabase = mongoDbClient.GetDatabase(databaseName);

                return mongoDatabase;
            }).Named<IMongoDatabase>(MongoDatabaseName).SingleInstance();

            builder.Register<IUserStore<IdentityUser>>(context =>
            {
                var usersCollection = context.ResolveNamed<IMongoDatabase>(MongoDatabaseName).GetCollection<IdentityUser>("Identity_Users");

                IndexChecks.EnsureUniqueIndexOnNormalizedEmail(usersCollection);
                IndexChecks.EnsureUniqueIndexOnNormalizedUserName(usersCollection);

                return new UserStore<IdentityUser>(usersCollection);
            }).SingleInstance();

            builder.Register<IRoleStore<IdentityRole>>(context =>
            {
                var rolesCollection = context.ResolveNamed<IMongoDatabase>(MongoDatabaseName).GetCollection<IdentityRole>("Identity_Roles");

                IndexChecks.EnsureUniqueIndexOnNormalizedRoleName(rolesCollection);

                return new RoleStore<IdentityRole>(rolesCollection);
            }).SingleInstance();

            builder.RegisterType<MongoUserRepository>()
                .As<IUserRepository>()
                .InstancePerLifetimeScope();

            builder.RegisterType<MongoDbStoresExternalSystem>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseName))
                .As<IExternalSystem>()
                .InstancePerLifetimeScope();

            builder.RegisterType<MongoPersistedGrantStore>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseName))
                .As<IPersistedGrantStore>()
                .SingleInstance();

            builder.RegisterType<MongoContentRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseName))
                .As<IContentRepository>()
                .As<ICatchEventConsumer>()
                .As<IReplayableStore>()
                .SingleInstance();

            builder.RegisterType<MongoHistoryEventRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseName))
                .As<IHistoryEventRepository>()
                .As<ICatchEventConsumer>()
                .As<IReplayableStore>()
                .SingleInstance();

            builder.RegisterType<MongoSchemaRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseName))
                .As<ISchemaRepository>()
                .As<ICatchEventConsumer>()
                .As<IReplayableStore>()
                .SingleInstance();

            builder.RegisterType<MongoAppRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseName))
                .As<IAppRepository>()
                .As<ICatchEventConsumer>()
                .As<IReplayableStore>()
                .SingleInstance();
        }
    }
}
