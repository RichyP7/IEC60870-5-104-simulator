using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.Domain.Interfaces
{
    public interface IIec104ConfigurationService
    {
        IReadOnlyDictionary<IecAddress, Iec104CommandDataPointConfig> CommandDataPoints { get; }
        bool CheckCommandExisting(IecAddress iecAddress);
        void ConfigureDataPoints(List<Iec104CommandDataPointConfig> commands);
        Iec104CommandDataPointConfig GetCommand(IecAddress iecAddress);
    }
}
