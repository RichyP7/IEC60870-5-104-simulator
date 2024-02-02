namespace IEC60870_5_104_simulator.Domain.Service
{
    public interface IIec104ConfigurationService
    {
        Dictionary<IecAddress, Iec104DataPointConfig> DataPoints { get; }

        bool CheckCommandExisting(IecAddress iecAddress);
        void ConfigureDataPoints();
        Iec104CommandDataPointConfig GetCommand(IecAddress iecAddress);
    }
}