namespace IEC60870_5_104_simulator.Domain.Interfaces
{
    /// <summary>
    /// Abstraction for publishing real-time scenario state events to connected UI clients.
    /// Implemented in the API layer using SignalR; Infrastructure depends only on this interface.
    /// </summary>
    public interface IScenarioEventPublisher
    {
        Task PublishScenarioUpdate(ScenarioState state);
        Task PublishDataPointChanged(Iec104DataPoint dataPoint);
    }
}
