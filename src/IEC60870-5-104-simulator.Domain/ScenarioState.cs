namespace IEC60870_5_104_simulator.Domain
{
    public enum ScenarioStatus { Idle, Running, Completed, Failed }

    /// <summary>Live state snapshot of a named scenario.</summary>
    public record ScenarioState(string Name, ScenarioStatus Status, int RemainingMs);
}
