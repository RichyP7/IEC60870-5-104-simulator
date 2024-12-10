using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using IEC60870_5_104_simulator.Infrastructure.Dto;

namespace IEC60870_5_104_simulator.Infrastructure.DTO.Mapper;

public class Iec104DataPointDtoMapper
{

    public Iec104DataPointDto MapToDto(Iec104DataPoint iec104DataPoint)
    {
        var dto = new Iec104DataPointDto()
        {
            Id = iec104DataPoint.Id,
            stationaryAddress = iec104DataPoint.Address.StationaryAddress,
            objectAddress = iec104DataPoint.Address.ObjectAddress,
            Iec104DataType = iec104DataPoint.Iec104DataType,
            Value = iec104DataPoint.Value.ToString(),
            Mode = iec104DataPoint.Mode,
        };

        return dto;
    }
    
    public Iec104DataPoint MapFromDto(Iec104DataPointDto iec104DataPointDto)
    {
        var dataPoint = new Iec104DataPoint()
        {
            Id = iec104DataPointDto.Id,
            Address = new IecAddress(iec104DataPointDto.stationaryAddress, iec104DataPointDto.objectAddress),
            Iec104DataType = iec104DataPointDto.Iec104DataType,
            Value = MapStringToValueObject(iec104DataPointDto.Value),
            Mode = iec104DataPointDto.Mode,
        };

        return dataPoint;
    }

    public IecValueObject MapStringToValueObject(String value)
    {
        if (IsInteger(value))
        {
            return new IecIntValueObject(int.Parse(value));
        }

        if (IsBoolean(value))
        {
            return new IecSinglePointValueObject(bool.Parse(value));
        }

        if (float.TryParse(value, out float floatValue))
        {
            return new IecValueFloatObject(floatValue);
        }

        else throw new NotImplementedException();
    }
    
    bool IsInteger(string value)
    {
        return int.TryParse(value, out _);
    }
    
    bool IsBoolean(string value)
    {
        return bool.TryParse(value, out _);
    }


    
}