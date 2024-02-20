namespace IEC60870_5_104_simulator.Domain.Service
{
    public interface IIec104Service
    {
        Task Simulate(IEnumerable<Iec104DataPoint> datapoints);
        Task Start();
        Task Stop();
        bool ConnectionEstablished();
    }
}