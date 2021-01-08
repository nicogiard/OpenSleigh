﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenSleigh.Core.DependencyInjection;
using OpenSleigh.Core.Messaging;
using OpenSleigh.Persistence.Mongo;
using OpenSleigh.Samples.Sample2.Common.Sagas;
using OpenSleigh.Transport.RabbitMQ;

namespace OpenSleigh.Samples.Sample2.Worker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var hostBuilder = CreateHostBuilder(args);
            var host = hostBuilder.Build();

            await host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddLogging(cfg =>
                    {
                        cfg.AddConsole();
                    })
                    .AddOpenSleigh(cfg =>
                    {
                        var mongoSection = hostContext.Configuration.GetSection("Mongo");
                        var mongoCfg = new MongoConfiguration(mongoSection["ConnectionString"], 
                                                              mongoSection["DbName"],
                                                              MongoSagaStateRepositoryOptions.Default);

                        var rabbitSection = hostContext.Configuration.GetSection("Rabbit");
                        var rabbitCfg = new RabbitConfiguration(rabbitSection["HostName"], 
                            rabbitSection["UserName"],
                            rabbitSection["Password"]);

                        cfg.AddSaga<ParentSaga, ParentSagaState>()
                            .UseStateFactory(msg => new ParentSagaState(msg.CorrelationId))
                            .UseRabbitMQTransport(rabbitCfg)
                            .UseMongoPersistence(mongoCfg);

                        cfg.AddSaga<ChildSaga, ChildSagaState>()
                            .UseStateFactory(msg => new ChildSagaState(msg.CorrelationId))
                            .UseRabbitMQTransport(rabbitCfg)
                            .UseMongoPersistence(mongoCfg);
                    });
            });
    }
}
