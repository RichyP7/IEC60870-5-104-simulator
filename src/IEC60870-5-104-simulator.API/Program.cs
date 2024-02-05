using IEC60870_5_104_simulator.API;
using IEC60870_5_104_simulator.API.Controllers;
using IEC60870_5_104_simulator.API.Mapping;
using IEC60870_5_104_simulator.Service;
using ServiceExtensionMethods;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<SimulationEngine>();
builder.Services.AddHostedService(provider => provider.GetService<SimulationEngine>() ?? throw new InvalidProgramException("Register Simulation Engine "));
builder.Services.AddServices();

Type t = typeof(IecConfigProfile);
builder.Services.AddAutoMapper(t.Assembly);

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

app.MapControllers();

app.Run();
