using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.Domain.Interfaces
{
    public interface IIec104ConfigurationService
    {
        Dictionary<IecAddress, Iec104DataPoint> DataPoints { get; }

        bool CheckCommandExisting(IecAddress iecAddress);
        void ConfigureDataPoints(List<Iec104CommandDataPointConfig> commands, List<Iec104DataPoint> datapoints);
        Iec104CommandDataPointConfig GetCommand(IecAddress iecAddress);
    }
}