using Autofac;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PashaInsuranceFiltering.Application.Abstractions;
using PashaInsuranceFiltering.Application.Common.Ports;
using PashaInsuranceFiltering.Application.Features.CQRS.Commands.UploadCommands;
using PashaInsuranceFiltering.Infrastructure.Background;
using PashaInsuranceFiltering.Infrastructure.Filtering;
using PashaInsuranceFiltering.Infrastructure.Messaging;
using PashaInsuranceFiltering.Infrastructure.Persistence.InMemory;
using PashaInsuranceFiltering.SharedKernel.Application.Behaviors;


namespace PashalinsuranceFiltering.DependencyInjection.DependencyResolvers
{
    public sealed class AutofacBusinessModule : Module
    {
        private readonly IConfiguration _config;

        public AutofacBusinessModule(IConfiguration config)
        {
            _config = config;
        }

        protected override void Load(ContainerBuilder container)
        {
            // Assembly
            var appAssembly = typeof(UploadChunkCommand).Assembly;      // Application
            var infraAssembly = typeof(IUploadBuffer).Assembly;    // Infrastructure

            // MediatR handlers
            container.RegisterAssemblyTypes(appAssembly)
                     .AsClosedTypesOf(typeof(IRequestHandler<,>))
                     .InstancePerLifetimeScope();

            //Pipeline Behaviors 
            container.RegisterGeneric(typeof(ValidationBehavior<,>))
                     .As(typeof(IPipelineBehavior<,>))
                     .InstancePerLifetimeScope();

            // FluentValidation validators
            container.RegisterAssemblyTypes(appAssembly)
                     .AsClosedTypesOf(typeof(IValidator<>))
                     .AsImplementedInterfaces()
                     .InstancePerLifetimeScope();

            // Ports - InMemory adapters
            container.RegisterType<InMemoryUploadBuffer>()
                     .As<IUploadBuffer>()
                     .SingleInstance();

            container.RegisterType<InMemoryTextDocumentRepository>()
                     .As<ITextDocumentRepository>()
                     .SingleInstance();

            // Queue
            container.RegisterType<InMemoryProcessingQueue>()
                     .AsSelf()
                     .As<IProcessingQueue>()
                     .SingleInstance();

            // Filter + metric
            var strategy = _config.GetValue<string>("Filtering:Strategy") ?? "JaroWinkler";
            if (string.Equals(strategy, "Levenshtein", StringComparison.OrdinalIgnoreCase))
                container.RegisterType<LevenshteinMetric>().As<ISimilarityMetric>().SingleInstance();
            else
                container.RegisterType<JaroWinklerMetric>().As<ISimilarityMetric>().SingleInstance();



            var banned = _config.GetSection("Filtering:BannedWords").Get<string[]>()
                     ?? Array.Empty<string>();

            container.Register(c => new InMemoryTextFilter(
                                   bannedWords: banned,
                                   metric: c.Resolve<ISimilarityMetric>()))
                     .As<ITextFilter>()
                     .SingleInstance();

            var threshold = _config.GetValue<double?>("Filtering:Threshold") ?? 0.8;

            // Background worker
            container.RegisterType<FilteringWorker>()
                     .WithParameter("threshold", threshold)
                     .As<IHostedService>()
                     .SingleInstance();


        }
    }
}
