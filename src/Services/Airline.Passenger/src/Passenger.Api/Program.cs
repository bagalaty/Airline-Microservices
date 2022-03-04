using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using BuildingBlocks.IdsGenerator;
using BuildingBlocks.Jwt;
using BuildingBlocks.Logging;
using BuildingBlocks.Mapster;
using BuildingBlocks.MassTransit;
using BuildingBlocks.Outbox;
using BuildingBlocks.Swagger;
using BuildingBlocks.Web;
using Figgle;
using FluentValidation;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Passenger;
using Passenger.Data;
using Passenger.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

Console.WriteLine(FiggleFonts.Standard.Render(configuration["app"]));

builder.Services.AddCustomDbContext<PassengerDbContext>(configuration, typeof(PassengerRoot).Assembly)
    .AddEntityFrameworkOutbox();

builder.AddCustomSerilog();
builder.Services.AddJwt();
builder.Services.AddControllers();
builder.Services.AddCustomSwagger(builder.Configuration, typeof(PassengerRoot).Assembly);
builder.Services.AddCustomVersioning();
builder.Services.AddCustomMediatR();
builder.Services.AddValidatorsFromAssembly(typeof(PassengerRoot).Assembly);
builder.Services.AddCustomProblemDetails();
builder.Services.AddCustomMapster(typeof(PassengerRoot).Assembly);
builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<IEventMapper, EventMapper>();

builder.Services.AddCustomMassTransit(typeof(PassengerRoot).Assembly);

SnowFlakIdGenerator.Configure(2);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    var provider = app.Services.GetService<IApiVersionDescriptionProvider>();
    app.UseCustomSwagger(provider);
}

app.UseSerilogRequestLogging();
app.UseCorrelationId();
app.UseProblemDetails();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", x => x.Response.WriteAsync(configuration["app"]));

app.Run();
