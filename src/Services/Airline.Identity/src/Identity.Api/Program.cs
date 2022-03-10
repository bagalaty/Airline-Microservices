using BuildingBlocks.CAP;
using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using BuildingBlocks.Logging;
using BuildingBlocks.Mapster;
using BuildingBlocks.MassTransit;
using BuildingBlocks.OpenTelemetry;
using BuildingBlocks.Outbox;
using BuildingBlocks.Persistence;
using BuildingBlocks.Swagger;
using BuildingBlocks.Utils;
using BuildingBlocks.Web;
using Figgle;
using FluentValidation;
using GreenPipes;
using Hellang.Middleware.ProblemDetails;
using Identity;
using Identity.Data;
using Identity.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var env = builder.Environment;

var appOptions = builder.Services.GetOptions<AppOptions>("AppOptions");
Console.WriteLine(FiggleFonts.Standard.Render(appOptions.Name));

builder.Services.AddScoped<IDbContext>(provider => provider.GetService<IdentityContext>()!);

builder.Services.AddDbContext<IdentityContext>(options =>
    options.UseSqlServer(
        configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsAssembly(typeof(IdentityRoot).Assembly.GetName().Name)))
    .AddEntityFrameworkOutbox();

builder.AddCustomSerilog();
builder.Services.AddControllers();
builder.Services.AddCustomSwagger(builder.Configuration, typeof(IdentityRoot).Assembly);
builder.Services.AddCustomVersioning();
builder.Services.AddCustomMediatR();
builder.Services.AddValidatorsFromAssembly(typeof(IdentityRoot).Assembly);
builder.Services.AddCustomProblemDetails();
builder.Services.AddCustomMapster(typeof(IdentityRoot).Assembly);
builder.Services.AddScoped<IDataSeeder, IdentityDataSeeder>();
builder.Services.AddTransient<IEventMapper, EventMapper>();

builder.Services.AddCustomMassTransit(typeof(IdentityRoot).Assembly);
builder.Services.AddCustomOpenTelemetry();

builder.Services.AddIdentityServer(env);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    var provider = app.Services.GetService<IApiVersionDescriptionProvider>();
    app.UseCustomSwagger(provider);
}

app.UseSerilogRequestLogging();
app.UseCorrelationId();
app.UseRouting();
app.UseHttpMetrics();
app.UseMigrations();
app.UseProblemDetails();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapMetrics();
});

app.MapGet("/", x => x.Response.WriteAsync(configuration["app"]));

app.Run();
