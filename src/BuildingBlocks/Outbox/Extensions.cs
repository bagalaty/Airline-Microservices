using System.Reflection;
using Ardalis.GuardClauses;
using BuildingBlocks.EFCore;
using BuildingBlocks.Outbox.EF;
using BuildingBlocks.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Outbox;

public static class Extensions
{
    public static IServiceCollection AddEntityFrameworkOutbox<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly migrationAssembly)
        where TContext : AppDbContextBase
    {
        var outboxOption = Guard.Against.Null(
            configuration.GetOptions<OutboxOptions>(nameof(OutboxOptions)),
            nameof(OutboxOptions));

        services.AddOptions<OutboxOptions>().Bind(configuration.GetSection(nameof(OutboxOptions)))
            .ValidateDataAnnotations();

        AppContext.SetSwitch("SqlServer.EnableLegacyTimestampBehavior", true);

        services.AddDbContext<TContext>(options =>
        {
            options.UseSqlServer(outboxOption.ConnectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(migrationAssembly.GetName().FullName);
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            }).UseSnakeCaseNamingConvention();
        });

        services.AddTransient<IOutboxService, EfOutboxService>();

        return services;
    }
}
