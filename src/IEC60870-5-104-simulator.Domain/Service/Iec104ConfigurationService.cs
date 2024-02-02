using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IEC60870_5_104_simulator.Domain.Service
{
    public class Iec104ConfigurationService : IIec104ConfigurationService
    {
        public Dictionary<IecAddress, Iec104DataPointConfig> DataPoints { get; }
        public Dictionary<IecAddress, Iec104CommandDataPointConfig> commandDataPoints;
        public Iec104ConfigurationService()
        {
            DataPoints = new();
            commandDataPoints = new();
            ConfigureDataPoints();
        }

        public void ConfigureDataPoints()
        {
            var address = new IecAddress(5, 123);
            DataPoints.Add(address, new Iec104DataPointConfig(address, Iec104DataTypes.M_SP_NA_1));

            var spaddress = new IecAddress(6, 123);
            Iec104DataPointConfig steppositionTest = new Iec104DataPointConfig(spaddress, Iec104DataTypes.M_ST_NA_1);
            DataPoints.Add(spaddress, steppositionTest);

            IecAddress commandAddress = new IecAddress(64798, 1);
            var firstcommand = new Iec104CommandDataPointConfig(commandAddress, Iec104DataTypes.C_RC_NA_1);
            firstcommand.AssignResponseDataPoint(steppositionTest);
            commandDataPoints.Add(commandAddress, firstcommand);
        }

        public Iec104CommandDataPointConfig GetCommand(IecAddress iecAddress)
        {
            if (commandDataPoints.TryGetValue(iecAddress, out Iec104CommandDataPointConfig value))
            {
                return value;
            }
            throw new KeyNotFoundException("no iec config for this iec address has been found");
        }
        public bool CheckCommandExisting(IecAddress iecAddress)
        {
            return commandDataPoints.TryGetValue(iecAddress, out _);

        }

    }
}
