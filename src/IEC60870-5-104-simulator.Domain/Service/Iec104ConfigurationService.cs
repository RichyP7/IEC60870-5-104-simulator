using System.Collections.Concurrent;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.Domain.Service
{
    /// <summary>
    /// Holds the configured IEC-104 datapoints and commands.
    /// </summary>
    public class Iec104ConfigurationService : IIec104ConfigurationService
    {
        private readonly ConcurrentDictionary<IecAddress, Iec104DataPoint> dataPoints = new();
        private readonly ConcurrentDictionary<IecAddress, Iec104CommandDataPointConfig> commandDataPoints = new();
        private readonly IIecValueRepository storage;

        public IReadOnlyDictionary<IecAddress, Iec104DataPoint> DataPoints => dataPoints;
        public IReadOnlyDictionary<IecAddress, Iec104CommandDataPointConfig> CommandDataPoints => commandDataPoints;

        public Iec104ConfigurationService(IIecValueRepository storage)
        {
            this.storage = storage;
        }

        public void ConfigureDataPoints(List<Iec104CommandDataPointConfig> commands, List<Iec104DataPoint> datapoints)
        {
            foreach (var data in datapoints)
            {
                if (!dataPoints.TryAdd(data.Address, data))
                    throw new ArgumentException($"Duplicate datapoint address {data.Address.StationaryAddress}/{data.Address.ObjectAddress}");
            }
            foreach (var command in commands)
            {
                if (!commandDataPoints.TryAdd(command.Address, command))
                    throw new ArgumentException($"Duplicate command address {command.Address.StationaryAddress}/{command.Address.ObjectAddress}");
            }
            InitStorage();
        }

        public bool AddConfiguredDataPoint(Iec104DataPoint dataPoint)
        {
            return dataPoints.TryAdd(dataPoint.Address, dataPoint);
        }

        private void InitStorage()
        {
            foreach (var data in dataPoints)
            {
                storage.AddDataPoint(data.Key, data.Value);
            }
        }

        public Iec104CommandDataPointConfig GetCommand(IecAddress iecAddress)
        {
            if (commandDataPoints.TryGetValue(iecAddress, out Iec104CommandDataPointConfig? value))
            {
                return value;
            }
            throw new KeyNotFoundException("no iec config for this iec address has been found");
        }

        public bool CheckCommandExisting(IecAddress iecAddress)
        {
            return commandDataPoints.ContainsKey(iecAddress);
        }
    }
}
