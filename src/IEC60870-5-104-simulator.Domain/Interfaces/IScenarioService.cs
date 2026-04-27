namespace IEC60870_5_104_simulator.Domain.Interfaces
{
    public interface IScenarioService
    {
        /// <summary>
        /// Triggers the named scenario asynchronously.
        /// Returns false if the scenario does not exist or is already running.
        /// </summary>
        Task<bool> TriggerAsync(string name, CancellationToken ct = default);

        IEnumerable<ScenarioState> GetStates();
    }
}
