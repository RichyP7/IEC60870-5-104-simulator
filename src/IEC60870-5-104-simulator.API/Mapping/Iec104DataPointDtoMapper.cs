using System.Globalization;
using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.API.Mapping;

public class Iec104DataPointDtoMapper
{


    public IecValueObject? MapStringToValueObject(String? value, Iec104DataTypes dataType)
    {
        if (value == null) return null;
        if (dataType.IsIntegerValue())
        {
            if (!IsInteger(value)) throw new FormatException("Value must be integer");
            return new IecIntValueObject(int.Parse(value));
        }

        if (dataType.IsSinglePoint())
        {
            if (!IsBoolean(value)) throw new FormatException("Value must be bool");
            return new IecSinglePointValueObject(bool.Parse(value));
        }

        if (dataType.IsFloatValue())
        {
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
            {
                throw new FormatException("Value must be Floating Point");
            }
            return new IecValueFloatObject(floatValue);
        }

        if (dataType.IsDoublePoint())
        {
            if (!IsDoublePointValue(value, out IecDoublePointValue parsedValue)) throw new FormatException("Value must be Double Point");
            return new IecDoublePointValueObject(parsedValue);
        }

        else throw new NotImplementedException();
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