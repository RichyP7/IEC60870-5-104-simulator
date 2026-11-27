using System.Collections.Concurrent;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.Domain.Service
{
    /// <summary>
    /// Holds the configured command datapoints.
    /// </summary>
    /// <remarks>
    /// Measures are owned by <see cref="DatapointRegistry"/>; this module holds only commands
    /// and their response links.
    /// </remarks>
    public class Iec104ConfigurationService : IIec104ConfigurationService
    {
        private readonly ConcurrentDictionary<IecAddress, Iec104CommandDataPointConfig> commandDataPoints = new();

        public IReadOnlyDictionary<IecAddress, Iec104CommandDataPointConfig> CommandDataPoints => commandDataPoints;

        public void ConfigureDataPoints(List<Iec104CommandDataPointConfig> commands)
        {
            foreach (var command in commands)
            {
                if (!commandDataPoints.TryAdd(command.Address, command))
                    throw new ArgumentException($"Duplicate command address {command.Address.StationaryAddress}/{command.Address.ObjectAddress}");
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
