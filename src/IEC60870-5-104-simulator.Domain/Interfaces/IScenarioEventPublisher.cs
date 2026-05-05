namespace IEC60870_5_104_simulator.Domain.Interfaces
{
    /// <summary>
    /// Abstraction for publishing real-time scenario state events to connected UI clients.
    /// </summary>
    public interface IScenarioEventPublisher
    {
        Task PublishScenarioUpdate(ScenarioState state);
        Task PublishDataPointChanged(Iec104DataPoint dataPoint);
    }
}
