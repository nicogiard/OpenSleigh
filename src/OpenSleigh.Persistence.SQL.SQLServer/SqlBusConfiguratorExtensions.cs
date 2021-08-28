﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenSleigh.Core.DependencyInjection;
using OpenSleigh.Core.Persistence;

namespace OpenSleigh.Persistence.SQL.SQLServer
{
    [ExcludeFromCodeCoverage]
    public static class SqlBusConfiguratorExtensions
    {
        public static IBusConfigurator UseSqlServerPersistence(
            this IBusConfigurator busConfigurator, SqlConfiguration config)
        {
            busConfigurator.Services.AddDbContextPool<SagaDbContext>(builder =>
            {
                builder.UseSqlServer(config.ConnectionString);
            }).AddScoped<ISagaDbContext>(ctx => ctx.GetRequiredService<SagaDbContext>())
            .AddScoped<ITransactionManager, SqlTransactionManager>()
            .AddSingleton(config.SagaRepositoryOptions)
            .AddSingleton(config.OutboxRepositoryOptions)
            .AddScoped<IOutboxRepository, SqlOutboxRepository>()
            .AddScoped<ISagaStateRepository, SqlSagaStateRepository>();
            
            return busConfigurator;
        }
    }
}