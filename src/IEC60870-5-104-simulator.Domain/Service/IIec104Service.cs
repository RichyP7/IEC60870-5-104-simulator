namespace IEC60870_5_104_simulator.Domain.Service
{
    public interface IIec104Service
    {
        Task SimulateCyclic(IEnumerable<Iec104DataPoint> datapoints);
        Task Simulate(Iec104DataPoint dataPoint);
        Task Start();
        Task Stop();
        bool ConnectionEstablished();
    }
}