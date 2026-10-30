using System.Globalization;
using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using Microsoft.Extensions.Options;
using static IEC60870_5_104_simulator.API.Iec104SimulationOptions;

namespace IEC60870_5_104_simulator.API.Services;

/// <summary>
/// Builds a SimulationOptions.json-compatible document from the current runtime state,
/// users can persist changes made via the UI (created/updated/deleted datapoints)
/// and restart the simulator with that file.
/// </summary>
public class ConfigurationExportService
{
    private readonly IIec104ConfigurationService _configurationService;
    private readonly IOptions<Iec104SimulationOptions> _options;

    public ConfigurationExportService(
        IIec104ConfigurationService configurationService,
        IOptions<Iec104SimulationOptions> options)
    {
        _configurationService = configurationService;
        _options = options;
    }

    public Dictionary<string, Iec104SimulationOptions> BuildExportDocument()
    {
        var current = _options.Value;

        var export = new Iec104SimulationOptions
        {
            CycleTimeMs = current.CycleTimeMs,
            Debug = current.Debug,
            MaxQueueSize = current.MaxQueueSize,
            DataPointConfiguration = new IecConfiguration
            {
                Measures = _configurationService.DataPoints.Values
                    .OrderBy(dp => dp.Address.StationaryAddress)
                    .ThenBy(dp => dp.Address.ObjectAddress)
                    .Select(ToMeasureConfig)
                    .ToList(),
                Commands = _configurationService.CommandDataPoints.Values
                    .OrderBy(c => c.Address.StationaryAddress)
                    .ThenBy(c => c.Address.ObjectAddress)
                    .Select(ToCommandConfig)
                    .ToList(),
            }
        };

        return new Dictionary<string, Iec104SimulationOptions>
        {
            [Iec104Simulation] = export
        };
    }

    private static DataPointConfig ToMeasureConfig(Iec104DataPoint dp) => new()
    {
        Id = dp.Id,
        Name = dp.Name,
        Ca = dp.Address.StationaryAddress,
        Oa = dp.Address.ObjectAddress,
        TypeId = (int)dp.Iec104DataType,
        Mode = (SimulationModeConfig)dp.Mode,
        InitValue = ToInitValue(dp), //current value to init
        ProfileValues = dp.ProfileValues,
        BaseValue = dp.BaseValue,
        MinValue = dp.MinValue,
        MaxValue = dp.MaxValue,
        FluctuationRate = dp.FluctuationRate,
        WavePeriodSeconds = dp.WavePeriodSeconds,
        LinkedDataPointId = dp.LinkedDataPointId,
    };

    private static string ToInitValue(Iec104DataPoint dp) => dp.Value switch
    {
        IecSinglePointValueObject sp => sp.Value ? "true" : "false",
        IecDoublePointValueObject dpv => dpv.Value.ToString(),
        IecValueScaledObject sv => sv.Value.Value.ToString(CultureInfo.InvariantCulture),
        IecValueFloatObject fv => fv.Value.ToString(CultureInfo.InvariantCulture),
        IecIntValueObject iv => iv.Value.ToString(CultureInfo.InvariantCulture),
        _ => dp.InitString ?? string.Empty,
    };

    private static CommandPointConfig ToCommandConfig(Iec104CommandDataPointConfig cmd) => new()
    {
        Id = cmd.Id,
        Name = cmd.Name,
        Ca = cmd.Address.StationaryAddress,
        Oa = cmd.Address.ObjectAddress,
        TypeId = (int)cmd.Iec104DataType,
        // ResponseId was resolved into SimulatedDataPoint at startup; invert that here.
        ResponseId = cmd.SimulatedDataPoint?.Id ?? string.Empty,
    };
}
