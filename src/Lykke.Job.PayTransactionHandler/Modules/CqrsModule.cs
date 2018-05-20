using System;
using System.Collections.Generic;
using Autofac;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Job.PayTransactionHandler.Commands;
using Lykke.Job.PayTransactionHandler.Core.Settings;
using Lykke.Job.PayTransactionHandler.Handlers;
using Lykke.Job.PayTransactionHandler.Sagas;
using Lykke.Messaging;
using Lykke.Messaging.RabbitMq;
using Lykke.Service.Operations.Contracts.Events;
using Lykke.SettingsReader;

namespace Lykke.Job.PayTransactionHandler.Modules
{
    public class CqrsModule : Module
    {
        private readonly AppSettings _settings;
        private readonly ILog _log;

        public CqrsModule(
            [NotNull] IReloadingManager<AppSettings> settingsManager, 
            [NotNull] ILog log)
        {
            _settings = settingsManager.CurrentValue ?? throw new ArgumentNullException(nameof(settingsManager.CurrentValue));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        protected override void Load(ContainerBuilder builder)
        {
            Messaging.Serialization.MessagePackSerializerFactory.Defaults.FormatterResolver =
                MessagePack.Resolvers.ContractlessStandardResolver.Instance;

            builder.Register(ctx => new AutofacDependencyResolver(ctx))
                .As<IDependencyResolver>();

            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory
                {Uri = _settings.Transports.ClientRabbitMqConnectionString};

            var messagingEngine = new MessagingEngine(_log,
                new TransportResolver(new Dictionary<string, TransportInfo>
                {
                    {
                        "ClientRabbitMq",
                        new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName,
                            rabbitMqSettings.Password, "None", "RabbitMq")
                    }
                }), new RabbitMqTransportFactory());

            var clientEndpointResolver = new RabbitMqConventionEndpointResolver("ClientRabbitMq", "messagepack",
                environment: "lykke", exclusiveQueuePostfix: "k8s");

            builder.RegisterType<LykkePaymentOperationSaga>().SingleInstance();

            builder.RegisterType<LykkePaymentCommandHandler>()
                .WithParameter(TypedParameter.From(_settings.PayTransactionHandlerJob.Blockchain.ConfirmationsToSucceed))
                .SingleInstance();

            builder.Register(ctx => new CqrsEngine(
                    _log,
                    ctx.Resolve<IDependencyResolver>(),
                    messagingEngine,
                    new DefaultEndpointProvider(),
                    true,
                    Register.DefaultEndpointResolver(clientEndpointResolver),
                    Register.BoundedContext(BoundedContexts.Payments)
                        .FailedCommandRetryDelay(_settings.PayTransactionHandlerJob.Cqrs.RetryDelayInMilliseconds)
                        .ListeningCommands(typeof(CreateLykkePaymentTransactionCommand))
                            .On(CqrsRoutes.Self)
                        .WithCommandsHandler<LykkePaymentCommandHandler>(),

                    Register.Saga<LykkePaymentOperationSaga>("pay-tx-handler.payment-saga")
                        .ListeningEvents(typeof(OperationCompletedEvent))
                            .From(BoundedContexts.Operations).On(CqrsRoutes.Events)
                        .PublishingCommands(typeof(CreateLykkePaymentTransactionCommand))
                            .To(BoundedContexts.Payments).With(CqrsRoutes.Commands)))
                .As<ICqrsEngine>()
                .SingleInstance()
                .AutoActivate();
        }

        internal class AutofacDependencyResolver : IDependencyResolver
        {
            private readonly IComponentContext _context;

            public AutofacDependencyResolver([NotNull] IComponentContext kernel)
            {
                _context = kernel ?? throw new ArgumentNullException(nameof(kernel));
            }

            public object GetService(Type type)
            {
                return _context.Resolve(type);
            }

            public bool HasService(Type type)
            {
                return _context.IsRegistered(type);
            }
        }
    }
}
