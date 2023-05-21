namespace IEC60870_5_104_simulator.Infrastructure
{
    public interface IIeC104ServerRunner
    {
        Task SimulateValues();
        Task Start();
        Task Stop();
    }
}