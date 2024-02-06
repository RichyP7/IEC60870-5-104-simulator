using System.ComponentModel.DataAnnotations;

namespace IEC60870_5_104_simulator.API
{
    public class Iec104SimulationOptions
    {
        public const string Iec104Simulation = "Iec104Simulation";

        [Range(0, 100000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int CycleTimeMs { get; set; }
        public IecConfiguration DataPointConfiguration { get; set; }


        public class IecConfiguration
        {
            public List<DataPointConfig> Measures { get; set; }
            public List<CommandPointConfig> Commands { get; set; }
        }
        public class DataPointConfig
        {
            public int Ca { get; set; }
            public int Oa { get; set; }
            public int TypeId { get; set; }
            public string Id { get; set; }
            public SimulationModeConfig Mode {get;set;}

        }
        public enum SimulationModeConfig
        {
            None,
            Cyclic,
            Random
        }
        public class CommandPointConfig
        {
            public int Ca { get; set; }
            public int Oa { get; set; }
            [Range(45, 87, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
            public int TypeId { get; set; }
            public string Id { get; set; }
            public string ResponseId { get; set; }
        }
    }
}
