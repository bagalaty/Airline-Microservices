using BuildingBlocks.Domain;
using BuildingBlocks.Outbox.EF;
using BuildingBlocks.Outbox.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Outbox;

public static class Extensions
{
    public static IServiceCollection AddEntityFrameworkOutbox(this IServiceCollection services)
    {
        services.AddHostedService<OutboxProcessorBackgroundService>();
        services.AddScoped<IInboxService, EfInboxService>();
        services.AddScoped<IOutboxService, EfOutboxService>();
        services.AddScoped<IEventProcessor, EventProcessor>();

        return services;
    }

    public static IServiceCollection AddInMemoryOutbox(this IServiceCollection services)
    {
        services.AddHostedService<OutboxProcessorBackgroundService>();
        services.AddSingleton<IInMemoryOutboxStore, InMemoryOutboxStore>();
        services.AddScoped<IOutboxService, InMemoryOutboxService>();
        services.AddScoped<IInboxService, InMemoryInboxService>();
        services.AddScoped<IEventProcessor, EventProcessor>();

        return services;
    }
}
