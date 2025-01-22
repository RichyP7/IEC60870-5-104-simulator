using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using IEC60870_5_104_simulator.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;

namespace IEC60870_5_104_simulator.Infrastructure.DataPointsService;

public class ValueService(IIecValueRepository iecValueRepository, ILogger<ValueService> logger)
{

    // This should toggle/change the value of digital points
    public void ToggleDigitalPointValue(IecAddress address)
    {
        var datapoint = iecValueRepository.GetDataPoint(address);
        if (datapoint.Iec104DataType == Iec104DataTypes.M_DP_NA_1)
        {
            IecDoublePointValue value = (IecDoublePointValue)datapoint.Value.GetValue();
            // increase value by one from DoublePointValue.cs enum (0,1,2,3)
            value = (IecDoublePointValue)(((int)value + 1) % 4);
            iecValueRepository.SetDoublePoint(address, value);
        
            logger.LogInformation("Double point value toggled");
            return;
        } else if (datapoint.Iec104DataType == Iec104DataTypes.M_SP_NA_1)
        {
            bool value = (bool)datapoint.Value.GetValue();
            iecValueRepository.SetSinglePoint(address, !value);
            
            logger.LogInformation("Single point value toggled");
            return;
            
        }
        throw new BadRequestException("IEC104DataType not supported for value toggle");
        
    }
}