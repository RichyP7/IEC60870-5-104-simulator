namespace IEC60870_5_104_simulator.API
{
    public class SimulationOptions
    {
        public const string Simulation = "Simulation";
        /// <summary>
        /// Cycle time in milliseconds (default 2000)
        /// </summary>
        public int CycleTimeMs { get; set; } = 2000; 
    }
}
