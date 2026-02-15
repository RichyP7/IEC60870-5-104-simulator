using System.Text.Json.Serialization;
using IEC60870_5_104_simulator.API;
using IEC60870_5_104_simulator.API.HealthChecks;
using IEC60870_5_104_simulator.API.Mapping;
using IEC60870_5_104_simulator.API.Services;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Infrastructure;
using IEC60870_5_104_simulator.Service;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using ServiceExtensionMethods;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<SimulationEngine>();
builder.Services.AddHostedService(provider => provider.GetService<SimulationEngine>() ?? throw new InvalidProgramException("Register Simulation Engine "));
builder.Services.AddServices();
builder.Services.AddTransient<DataPointConfigService>();
builder.Services.AddTransient<DataPointValueService>();
builder.Services.AddHealthChecks()
    .AddCheck<ConnectionEstablishedHealthCheck>("readiness",HealthStatus.Healthy, new List<string> { "ready"})
    .AddCheck<ServerStartedHealthCheck>("liveness");
builder.Services.AddSingleton<ServerStartedHealthCheck>();


var allowedOrigins = builder.Configuration.GetSection("CorsPolicies:AllowAngularApp:Origins").Get<string[]>()
    ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
Type t = typeof(IecConfigProfile);
builder.Services.AddAutoMapper(t.Assembly);

builder.Configuration.AddJsonFile("Configuration/SimulationOptions.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("Configuration/Profiles.json", optional: true, reloadOnChange: true);
builder.Services.AddOptions<Iec104SimulationOptions>().Bind(
    builder.Configuration.GetSection(Iec104SimulationOptions.Iec104Simulation))
    .ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<ProfilesOptions>().Bind(
    builder.Configuration);
builder.Services.AddSingleton<IProfileProvider>(sp =>
{
    var profilesOptions = sp.GetRequiredService<IOptions<ProfilesOptions>>().Value;
    return new ProfileProvider(profilesOptions.Profiles);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");;
app.UseExceptionHandler(new ExceptionHandlerOptions()
{
    AllowStatusCode404Response = true,
    ExceptionHandlingPath = "/error"
});
app.UseAuthorization();
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = healthCheck => healthCheck.Tags.Contains("ready") });
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = healthCheck => !healthCheck.Tags.Contains("ready") }); //all but ready

app.MapControllers();

app.Run();

public partial class Program { }