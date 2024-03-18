using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.Domain.Service
{
    public class Iec104ConfigurationService : IIec104ConfigurationService
    {
        public Dictionary<IecAddress, Iec104DataPoint> DataPoints { get; }
        public Dictionary<IecAddress, Iec104CommandDataPointConfig> commandDataPoints;
        private readonly IIecValueRepository storage;

        public Iec104ConfigurationService(IIecValueRepository storage)
        {
            DataPoints = new();
            commandDataPoints = new();
            this.storage = storage;
        }

        public void ConfigureDataPoints(List<Iec104CommandDataPointConfig> commands, List<Iec104DataPoint> datapoints)
        {
            foreach (var data in datapoints)
            {
                DataPoints.Add(data.Address, data);
            }
            foreach (var command in commands)
            {
                commandDataPoints.Add(command.Address, command);
            }
            InitStorage();

        }
        private void InitStorage()
        {

            foreach (var data in DataPoints)
            {

                storage.AddDataPoint(data.Key, data.Value);
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
