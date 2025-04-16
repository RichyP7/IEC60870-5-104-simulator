using System.Globalization;
using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.API.Mapping;

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
            //Value = iec104DataPoint.Value.ToString(),
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
            //Value = MapStringToValueObject(iec104DataPointDto.Value, iec104DataPointDto.Iec104DataType)!,
            Mode = iec104DataPointDto.Mode,
        };

        return dataPoint;
    }

    public IecValueObject? MapStringToValueObject(String? value, Iec104DataTypes dataType)
    {
        if (value == null) return null;
        if (DataTypeIsInteger(dataType))
        {
            if (!IsInteger(value)) throw new FormatException("Value must be integer");
            return new IecIntValueObject(int.Parse(value));
        }

        if (DataTypeIsSinglePoint(dataType))
        {
            if (!IsBoolean(value)) throw new FormatException("Value must be bool");
            return new IecSinglePointValueObject(bool.Parse(value));
        }

        if (DataTypeIsFloatingPoint(dataType))
        {
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
            {
                throw new FormatException("Value must be Floating Point");
            }
            return new IecValueFloatObject(floatValue);
        }

        if (DataTypeIsDoublePoint(dataType))
        {
            if (!IsDoublePointValue(value, out IecDoublePointValue parsedValue)) throw new FormatException("Value must be Double Point");
            return new IecDoublePointValueObject(parsedValue);
        }

        else throw new NotImplementedException();
    }

    private static bool DataTypeIsInteger(Iec104DataTypes dataType)
    {
        return dataType is 
            Iec104DataTypes.M_ST_NA_1 or
            Iec104DataTypes.M_ST_TA_1 or
            Iec104DataTypes.M_ST_TB_1 or
            Iec104DataTypes.M_ME_NB_1 or
            Iec104DataTypes.M_ME_TB_1 or 
            Iec104DataTypes.M_ME_TE_1;
    }
    
    private static bool DataTypeIsSinglePoint(Iec104DataTypes dataType)
    {
        return dataType is
            Iec104DataTypes.M_SP_NA_1 or
            Iec104DataTypes.M_SP_TA_1 or
            Iec104DataTypes.M_SP_TB_1;
    }
    
    private static bool DataTypeIsDoublePoint(Iec104DataTypes dataType)
    {
        return dataType is
            Iec104DataTypes.M_DP_NA_1 or
            Iec104DataTypes.M_DP_TA_1 or
            Iec104DataTypes.M_DP_TB_1;
    }
    
    private static bool DataTypeIsFloatingPoint(Iec104DataTypes dataType)
    {
        return dataType is
            Iec104DataTypes.M_ME_NC_1 or
            Iec104DataTypes.M_ME_TC_1 or
            Iec104DataTypes.M_ME_TF_1 or
            Iec104DataTypes.M_ME_NA_1 or
            Iec104DataTypes.M_ME_TA_1 or
            Iec104DataTypes.M_ME_ND_1;
    }

    private static bool IsInteger(string value)
    {
        return int.TryParse(value, out _);
    }
    
    private static bool IsBoolean(string value)
    {
        return bool.TryParse(value, out _);
    }

    private bool IsDoublePointValue(string value, out IecDoublePointValue result)
    {
        return Enum.TryParse(value, true, out result);
    }



    
}