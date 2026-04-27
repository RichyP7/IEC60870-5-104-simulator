namespace IEC60870_5_104_simulator.Domain
{
    /// <summary>
    /// A named fault scenario consisting of ordered timeline steps and auto-recovery steps.
    /// </summary>
    public record ScenarioDefinition(
        string Name,
        int RecoveryMs,
        IReadOnlyList<ScenarioStep> Steps,
        IReadOnlyList<ScenarioStep> RecoverySteps);
}
