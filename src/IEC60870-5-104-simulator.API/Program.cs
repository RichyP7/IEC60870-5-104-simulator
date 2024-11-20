using IEC60870_5_104_simulator.API;
using IEC60870_5_104_simulator.API.Controllers;
using IEC60870_5_104_simulator.API.HealthChecks;
using IEC60870_5_104_simulator.API.Mapping;
using IEC60870_5_104_simulator.Service;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ServiceExtensionMethods;
using System.Reflection;
using IEC60870_5_104_simulator.API.JsonConverter;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new IecValueObjectConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<SimulationEngine>();
builder.Services.AddHostedService(provider => provider.GetService<SimulationEngine>() ?? throw new InvalidProgramException("Register Simulation Engine "));
builder.Services.AddServices();
builder.Services.AddHealthChecks()
    .AddCheck<ConnectionEstablishedHealthCheck>("readiness",HealthStatus.Healthy, new List<string> { "ready"})
    .AddCheck<ServerStartedHealthCheck>("liveness");
builder.Services.AddSingleton<ServerStartedHealthCheck>();

Type t = typeof(IecConfigProfile);
builder.Services.AddAutoMapper(t.Assembly);

builder.Configuration.AddJsonFile("Configuration/SimulationOptions.json", optional: true, reloadOnChange: true);
builder.Services.Configure<Iec104SimulationOptions>(
    builder.Configuration.GetSection(Iec104SimulationOptions.Iec104Simulation));



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = healthCheck => healthCheck.Tags.Contains("ready") });
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = healthCheck => !healthCheck.Tags.Contains("ready") }); //all but ready

app.MapControllers();

app.Run();
