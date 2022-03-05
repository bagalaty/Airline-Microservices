using System.Reflection;
using BuildingBlocks.Domain.Event;
using BuildingBlocks.Utils;
using BuildingBlocks.Web;
using Humanizer;
using Jaeger.Samplers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Jaeger;
using MassTransit.OpenTracing;
using MassTransit.PrometheusIntegration;
using OpenTracing.Util;


namespace BuildingBlocks.MassTransit;

public static class Extensions
{
    private static bool? _isRunningInContainer;

    private static bool IsRunningInContainer => _isRunningInContainer ??=
        bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inContainer) &&
        inContainer;

    public static IServiceCollection AddCustomMassTransit(this IServiceCollection services, Assembly assembly)
    {
        services.AddMassTransit(configure =>
        {
            configure.AddConsumers(assembly);

            configure.UsingRabbitMq((context, configurator) =>
            {
                var rabbitMqOptions = services.GetOptions<RabbitMqOptions>("RabbitMq");
                var host = IsRunningInContainer ? "rabbitmq" : rabbitMqOptions.HostName;

                configurator.Host(host, h =>
                {
                    h.Username(rabbitMqOptions.UserName);
                    h.Password(rabbitMqOptions.Password);
                });

                configurator.PropagateOpenTracingContext();
                configurator.UsePrometheusMetrics(serviceName: services.GetOptions<AppOptions>("AppOptions")?.Name ?? "Unknown");

                var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                    .Where(x => x.IsAssignableTo(typeof(IIntegrationEvent))
                                && !x.IsInterface
                                && !x.IsAbstract
                                && !x.IsGenericType);

                foreach (var type in types)
                {
                    var consumers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                        .Where(x => x.IsAssignableTo(typeof(IConsumer<>).MakeGenericType(type))).ToList();

                    if (consumers.Any())
                    {
                        configurator.ReceiveEndpoint(
                            string.IsNullOrEmpty(rabbitMqOptions.ExchangeName)
                                ? type.Name.Underscore()
                                : $"{rabbitMqOptions.ExchangeName}_{type.Name.Underscore()}", e =>
                            {
                                foreach (var consumer in consumers)
                                {
                                    configurator.ConfigureEndpoints(context, x => x.Exclude(consumer));
                                    var methodInfo = typeof(DependencyInjectionReceiveEndpointExtensions)
                                        .GetMethods()
                                        .Where(x => x.GetParameters()
                                            .Any(p => p.ParameterType == typeof(IServiceProvider)))
                                        .FirstOrDefault(x => x.Name == "Consumer" && x.IsGenericMethod);

                                    var generic = methodInfo?.MakeGenericMethod(consumer);
                                    generic?.Invoke(e, new object[] {e, context, null});
                                }
                            });
                    }
                }
            });
        });

        services.AddMassTransitHostedService();

        // go to http://localhost:16686 for Jaeger
        services.AddOpenTracing();

        var tracer = new Tracer.Builder(services.GetOptions<AppOptions>("AppOptions")?.Name ?? "Unknown")
            .WithSampler(new ConstSampler(true))
            .Build();
        GlobalTracer.Register(tracer);

        return services;
    }
}
