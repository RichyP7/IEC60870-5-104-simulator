namespace IEC60870_5_104_simulator.API
{
    public class ScenariosOptions
    {
        public const string SectionName = "Scenarios";
        public List<ScenarioDefinitionConfig> Scenarios { get; set; } = new();

        public class ScenarioDefinitionConfig
        {
            public string Name { get; set; } = "";
            public int RecoveryMs { get; set; } = 5000;
            public List<ScenarioStepConfig> Steps { get; set; } = new();
            public List<ScenarioStepConfig> RecoverySteps { get; set; } = new();
        }

        public class ScenarioStepConfig
        {
            public int DelayMs { get; set; }
            public int Ca { get; set; }
            public int Oa { get; set; }
            /// <summary>
            /// String representation of the value — same parsing rules as InitValue:
            /// "true"/"false" for single-point, "ON"/"OFF"/"INTERMEDIATE"/"INDETERMINATE" for double-point,
            /// integer for step/scaled, float (invariant culture) for short/normalized.
            /// </summary>
            public string ValueStr { get; set; } = "";
            public bool Freeze { get; set; }
            public string Description { get; set; } = "";
        }
    }
}
