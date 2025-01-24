using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using IEC60870_5_104_simulator.Infrastructure.Dto;
using IEC60870_5_104_simulator.Infrastructure.DTO.Mapper;

namespace IEC60870_5_104_simulator.Infrastructure.DataPointsService;

public class DataPointService
{
    private IIecValueRepository _iecValueRepository;
    private Iec104DataPointDtoMapper mapper = new Iec104DataPointDtoMapper();

    public DataPointService(IIecValueRepository iecValueRepository)
    {
        _iecValueRepository = iecValueRepository;
    }

    public List<Iec104DataPointDto> GetAllDataPoints()
    {
        var data = _iecValueRepository.GetAllDataPoints();

        var dataPointDtos = data.Select(mapper.MapToDto).ToList();
        return dataPointDtos;
    }

    public Iec104DataPoint CreateDataPoint(Iec104DataPointDto dataPointDto)
    {
        var dataPoint = mapper.MapFromDto(dataPointDto);
        _iecValueRepository.AddDataPoint(dataPoint.Address, dataPoint);
        return dataPoint;

    }

    public Iec104DataPointDto UpdateSimulationMode(IecAddress address, SimulationMode mode)
    {
        _iecValueRepository.GetDataPoint(address);
        _iecValueRepository.SetSimulationMode(address, mode);
        return mapper.MapToDto(_iecValueRepository.GetDataPoint(address));
    }

    public Iec104DataPointDto GetDataPoint(IecAddress id)
    {
        return mapper.MapToDto(_iecValueRepository.GetDataPoint(id));
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