namespace IEC60870_5_104_simulator.Domain
{
    public interface IIec104Service
    {
        Task SimulateValues();
        Task Start(Iec104DataPointConfiguration config );
        Task Stop();
    }
}