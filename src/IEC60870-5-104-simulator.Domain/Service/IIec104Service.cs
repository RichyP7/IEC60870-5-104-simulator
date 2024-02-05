namespace IEC60870_5_104_simulator.Domain.Service
{
    public interface IIec104Service
    {
        Task Simulate();
        Task Start();
        Task Stop();
    }
}