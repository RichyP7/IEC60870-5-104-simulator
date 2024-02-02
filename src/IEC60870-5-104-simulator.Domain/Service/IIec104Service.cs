namespace IEC60870_5_104_simulator.Domain.Service
{
    public interface IIec104Service
    {
        Task SimulateValues();
        Task Start();
        Task Stop();
    }
}