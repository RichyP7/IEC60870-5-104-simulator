namespace IEC60870_5_104_simulator.Domain.Service
{
    public interface IIec104ConfigurationService
    {
        Dictionary<IecAddress, Iec104DataPointConfig> DataPoints { get; }

        bool CheckCommandExisting(IecAddress iecAddress);
        void ConfigureDataPoints(List<Iec104CommandDataPointConfig> commands, List<Iec104DataPointConfig> datapoints);
        Iec104CommandDataPointConfig GetCommand(IecAddress iecAddress);
    }
}