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
        }

        public void ConfigureDataPoints(List<Iec104CommandDataPointConfig> commands, List<Iec104DataPointConfig> datapoints)
        {
            foreach (var data in datapoints)
            {
                DataPoints.Add(data.Address, data);
            }
            foreach (var command in commands)
            {
                commandDataPoints.Add(command.Address, command);
            }
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
