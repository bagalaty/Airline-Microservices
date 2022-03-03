using BuildingBlocks.CAP;
using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using BuildingBlocks.Logging;
using BuildingBlocks.Mapster;
using BuildingBlocks.MassTransit;
using BuildingBlocks.Outbox;
using BuildingBlocks.Persistence;
using BuildingBlocks.Swagger;
using BuildingBlocks.Web;
using Figgle;
using FluentValidation;
using Hellang.Middleware.ProblemDetails;
using Identity;
using Identity.Data;
using Identity.Extensions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var env = builder.Environment;

Console.WriteLine(FiggleFonts.Standard.Render(configuration["app"]));

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

builder.Services.AddIdentityServer(env);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    var provider = app.Services.GetService<IApiVersionDescriptionProvider>();
    app.UseCustomSwagger(provider);
}

app.UseSerilogRequestLogging();
app.UseMigrations();
app.UseProblemDetails();
app.UseHttpsRedirection();
app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", x => x.Response.WriteAsync(configuration["app"]));

app.Run();
