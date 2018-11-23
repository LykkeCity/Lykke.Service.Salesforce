using System.Collections.Generic;
using System.Linq;
using Autofac;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Messaging;
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Serialization;
using Lykke.Service.Salesforce.Contract;
using Lykke.Service.Salesforce.Contract.Commands;
using Lykke.Service.Salesforce.Settings;
using Lykke.Service.Salesforce.Workflow.CommandHandlers;
using Lykke.SettingsReader;

namespace Lykke.Service.Salesforce.Modules
{
    [UsedImplicitly]
    public class CqrsModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public CqrsModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            string commandsRoute = "commands";
            
            MessagePackSerializerFactory.Defaults.FormatterResolver = MessagePack.Resolvers.ContractlessStandardResolver.Instance;
            var rabbitMqSagasSettings = new RabbitMQ.Client.ConnectionFactory { Uri = _settings.CurrentValue.SalesforceService.SagasRabbitMq.RabbitConnectionString };

            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>();
            
            builder.RegisterType<SalesforceCommandsHandler>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.SalesforceService.SagasRabbitMq.RetryDelay))
                .SingleInstance();

            builder.Register(ctx =>
                {
                    var logFactory = ctx.Resolve<ILogFactory>();

                    var messagingEngine = new MessagingEngine(logFactory,
                        new TransportResolver(new Dictionary<string, TransportInfo>
                        {
                            {
                                "SagasRabbitMq",
                                new TransportInfo(rabbitMqSagasSettings.Endpoint.ToString(),
                                    rabbitMqSagasSettings.UserName, rabbitMqSagasSettings.Password, "None", "RabbitMq")
                            }
                        }),
                        new RabbitMqTransportFactory(logFactory));

                    var sagasEndpointResolver = new RabbitMqConventionEndpointResolver(
                        "SagasRabbitMq",
                        SerializationFormat.MessagePack,
                        environment: "lykke",
                        exclusiveQueuePostfix: "k8s");

                    var commands = typeof(SalesforceBoundedContext).Assembly
                        .GetTypes()
                        .Where(x => x.Namespace == typeof(CreateContactCommand).Namespace)
                        .ToArray();

                    return new CqrsEngine(logFactory,
                        new AutofacDependencyResolver(ctx.Resolve<IComponentContext>()),
                        messagingEngine,
                        new DefaultEndpointProvider(),
                        true,
                        Register.DefaultEndpointResolver(sagasEndpointResolver),

                        Register.BoundedContext(SalesforceBoundedContext.Name)
                            .ListeningCommands(commands)
                            .On(commandsRoute)
                            .WithEndpointResolver(sagasEndpointResolver)
                            .WithCommandsHandler<SalesforceCommandsHandler>()
                            .ProcessingOptions(commandsRoute).MultiThreaded(4).QueueCapacity(1024)
                    );
                })
                .As<ICqrsEngine>()
                .SingleInstance();
        }
    }
}
