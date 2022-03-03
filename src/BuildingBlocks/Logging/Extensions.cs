using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Configuration;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.SpectreConsole;

namespace BuildingBlocks.Logging;

public static class HostBuilderExtensions
{
    public static WebApplicationBuilder AddCustomSerilog(
        this WebApplicationBuilder builder,
        Action<LoggerConfiguration>? extraConfigure = null)
    {
        AddCustomSerilog(builder.Host, extraConfigure);

        return builder;
    }

    public static IHostBuilder AddCustomSerilog(
        this IHostBuilder builder,
        Action<LoggerConfiguration>? extraConfigure = null)
    {
        return builder.UseSerilog((context, serviceProvider, loggerConfiguration) =>
        {
            var httpContext = serviceProvider.GetService<IHttpContextAccessor>();
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(serviceProvider)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error)
                .Enrich.WithTraceId(httpContext)
                .Enrich.WithSpan()
                .Enrich.FromLogContext();

            var loggerOptions = context.Configuration.GetSection(nameof(LoggerOptions)).Get<LoggerOptions>();
            if (loggerOptions is { })
                MapOptions(loggerOptions, loggerConfiguration, context);

            extraConfigure?.Invoke(loggerConfiguration);
        });
    }

    private static void MapOptions(
        LoggerOptions loggerOptions,
        LoggerConfiguration loggerConfiguration,
        HostBuilderContext hostBuilderContext)
    {
        var level = GetLogEventLevel(loggerOptions.Level);

        loggerConfiguration
            .MinimumLevel.Is(level)
            .Enrich.WithProperty("Environment", hostBuilderContext.HostingEnvironment.EnvironmentName);

        if (hostBuilderContext.HostingEnvironment.IsDevelopment())
        {
            loggerConfiguration.WriteTo.SpectreConsole(
                loggerOptions.LogTemplate ??
                "{Timestamp:HH:mm:ss} [{Level:u4}] {Message:lj}{NewLine}{Exception}",
                level);
        }
        else
        {
            if (loggerOptions.UseElasticSearch)
                loggerConfiguration.WriteTo.Elasticsearch(loggerOptions.ElasticSearchLoggingOptions?.Url);
            if (loggerOptions.UseSeq)
            {
                loggerConfiguration.WriteTo.Seq(Environment.GetEnvironmentVariable("SEQ_URL") ??
                                                loggerOptions.SeqOptions.Url);
            }

            loggerConfiguration.WriteTo.SpectreConsole(
                loggerOptions.LogTemplate ??
                "{Timestamp:HH:mm:ss} [{Level:u4}] {Message:lj}{NewLine}{Exception}",
                level);
        }

        foreach (var (key, value) in loggerOptions.Tags ?? new Dictionary<string, object>())
            loggerConfiguration.Enrich.WithProperty(key, value);

        foreach (var (key, value) in loggerOptions.MinimumLevelOverrides ?? new Dictionary<string, string>())
        {
            var logLevel = GetLogEventLevel(value);
            loggerConfiguration.MinimumLevel.Override(key, logLevel);
        }

        loggerOptions.ExcludePaths?.ToList().ForEach(p => loggerConfiguration.Filter
            .ByExcluding(Matching.WithProperty<string>("RequestPath", n => n.EndsWith(p, StringComparison.Ordinal))));

        loggerOptions.ExcludeProperties?.ToList().ForEach(p => loggerConfiguration.Filter
            .ByExcluding(Matching.WithProperty(p)));
    }

    private static LogEventLevel GetLogEventLevel(string level)
    {
        return Enum.TryParse<LogEventLevel>(level, true, out var logLevel)
            ? logLevel
            : LogEventLevel.Information;
    }

    internal static LoggerConfiguration WithTraceId(
        this LoggerEnrichmentConfiguration loggerEnrichmentConfiguration,
        IHttpContextAccessor httpContextAccessor)
    {
        if (loggerEnrichmentConfiguration == null)
            throw new ArgumentNullException(nameof(loggerEnrichmentConfiguration));

        if (httpContextAccessor == null)
            throw new ArgumentNullException(nameof(httpContextAccessor));

        return loggerEnrichmentConfiguration.With(new TraceIdEnricher(httpContextAccessor));
    }
}
