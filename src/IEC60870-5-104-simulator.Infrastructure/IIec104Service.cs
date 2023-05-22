namespace IEC60870_5_104_simulator.Infrastructure
{
    public interface IIec104Service
    {
        Task SimulateValues();
        Task Start();
        Task Stop();
    }
}