using AutoMapper;
using IEC60870_5_104_simulator.API.Mapping;
using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.API.Services;

public class DataPointConfigService
{
    private readonly IIecValueRepository _iecValueRepository;
    private IIec104ConfigurationService _configurationService;
    private readonly IMapper mapper;

    public DataPointConfigService(IIecValueRepository iecValueRepository, IIec104ConfigurationService configurationService, IMapper mapper)
    {
        _iecValueRepository = iecValueRepository;
        _configurationService = configurationService;
        this.mapper = mapper;
    }

    public List<Iec104DataPointDto> GetAllDataPoints()
    {
        var data = _iecValueRepository.GetAllDataPoints();
        return mapper.Map<IEnumerable<Iec104DataPointDto>>(data).ToList();
    }

    public Iec104DataPoint CreateDataPoint(Iec104DataPointDto dataPointDto)
    {
        var dataPoint = mapper.Map< Iec104DataPoint>(dataPointDto);
        _iecValueRepository.AddDataPoint(dataPoint.Address, dataPoint);
        _configurationService.AddConfiguredDataPoint(dataPoint);
        return dataPoint;
    }

    public Iec104DataPointDto UpdateSimulationMode(IecAddress address, SimulationMode mode, string? profileName = null)
    {
        var dataPoint = _iecValueRepository.GetDataPointValue(address);
        _iecValueRepository.SetSimulationMode(address, mode);
        dataPoint.ProfileName = profileName;
        return mapper.Map<Iec104DataPointDto>(_iecValueRepository.GetDataPointValue(address));
    }

    public Iec104DataPointDto GetDataPoint(IecAddress id)
    {
        return mapper.Map<Iec104DataPointDto>(_iecValueRepository.GetDataPointValue(id));
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
}