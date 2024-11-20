using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.Infrastructure.DataPointsService;

public class DataPointService
{
    private IIecValueRepository _iecValueRepository;

    public DataPointService(IIecValueRepository iecValueRepository)
    {
        _iecValueRepository = iecValueRepository;
    }

    public List<Iec104DataPoint> GetAllDataPoints()
    {
        var data = _iecValueRepository.GetAllDataPoints();
        var datapoints = data.Values.ToList();
        return datapoints;
    }

    public Iec104DataPoint CreateDataPoint(Iec104DataPoint dataPoint)
    {
        _iecValueRepository.AddDataPoint(dataPoint.Address, dataPoint);
        return dataPoint;

    }

    public Iec104DataPoint GetDataPoint(IecAddress id)
    {
        var found = _iecValueRepository.GetAllDataPoints().TryGetValue(id, out var value);
        if (!found || value == null) throw new KeyNotFoundException("DataPoint not found for id" + id);
        return value;
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