using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using BuildingBlocks.IdsGenerator;
using BuildingBlocks.Jwt;
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
using Flight;
using Flight.Data;
using Flight.Data.Seed;
using Flight.Extensions;
using FluentValidation;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Prometheus;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var appOptions = builder.Services.GetOptions<AppOptions>("AppOptions");
Console.WriteLine(FiggleFonts.Standard.Render(appOptions.Name));

builder.Services.AddCustomDbContext<FlightDbContext>(configuration, typeof(FlightRoot).Assembly)
    .AddEntityFrameworkOutbox();

builder.Services.AddScoped<IDataSeeder, FlightDataSeeder>();

builder.AddCustomSerilog();
builder.Services.AddJwt();
builder.Services.AddControllers();
builder.Services.AddCustomSwagger(builder.Configuration, typeof(FlightRoot).Assembly);
builder.Services.AddCustomVersioning();
builder.Services.AddCustomMediatR();
builder.Services.AddValidatorsFromAssembly(typeof(FlightRoot).Assembly);
builder.Services.AddCustomProblemDetails();
builder.Services.AddCustomMapster(typeof(FlightRoot).Assembly);
builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<IEventMapper, EventMapper>();

builder.Services.AddCustomMassTransit(typeof(FlightRoot).Assembly);
builder.Services.AddCustomOpenTelemetry();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

SnowFlakIdGenerator.Configure(1);


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
