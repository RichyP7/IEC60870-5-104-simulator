using IEC60870_5_104_simulator.API.Mapping;
using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.API.Services;

public class DataPointConfigService
{
    private readonly IIecValueRepository _iecValueRepository;
    private IIec104ConfigurationService _configurationService;

    public DataPointConfigService(IIecValueRepository iecValueRepository, IIec104ConfigurationService configurationService)
    {
        _iecValueRepository = iecValueRepository;
        _configurationService = configurationService;
    }

    public List<Iec104DataPointDto> GetAllDataPoints()
    {
        var data = _iecValueRepository.GetAllDataPoints();
        return data.Select(dp => dp.ToDto()).ToList();
    }

    public Iec104DataPoint CreateDataPoint(Iec104DataPointDto dataPointDto)
    {
        var dataPoint = dataPointDto.ToDomain();
        _iecValueRepository.AddDataPoint(dataPoint.Address, dataPoint);
        _configurationService.AddConfiguredDataPoint(dataPoint);
        return dataPoint;
    }

    public Iec104DataPointDto UpdateSimulationMode(IecAddress address, SimulationMode mode)
    {
        _iecValueRepository.GetDataPointValue(address);
        _iecValueRepository.SetSimulationMode(address, mode);
        return _iecValueRepository.GetDataPointValue(address).ToDto();
    }

    public Iec104DataPointDto GetDataPoint(IecAddress id)
    {
        return _iecValueRepository.GetDataPointValue(id).ToDto();
    }

    public bool DeleteDataPoint(int idStationary, int idObject)
    {
        var address = new IecAddress(idStationary, idObject);
        try
        {
            _iecValueRepository.DeleteDataPoint(address);
            return true;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

    /// <summary>
    /// Updates simulation parameters (BaseValue, MinValue, MaxValue, FluctuationRate,
    /// LinkedDataPointId, Mode) on an existing datapoint without changing the IEC-104 type.
    /// </summary>
    public void UpdateDataPointParams(IecAddress address, Iec104DataPointDto dto)
    {
        var dp = _iecValueRepository.GetDataPointValue(address);
        if (dto.Mode != default)
            dp.Mode = (SimulationMode)dto.Mode;
        if (dto.BaseValue.HasValue)
            dp.BaseValue = dto.BaseValue;
        if (dto.MinValue.HasValue)
            dp.MinValue = dto.MinValue;
        if (dto.MaxValue.HasValue)
            dp.MaxValue = dto.MaxValue;
        if (dto.FluctuationRate.HasValue)
            dp.FluctuationRate = dto.FluctuationRate;
        if (dto.WavePeriodSeconds.HasValue)
            dp.WavePeriodSeconds = dto.WavePeriodSeconds;
        if (dto.LinkedDataPointId != null)
            dp.LinkedDataPointId = dto.LinkedDataPointId;
    }
}
