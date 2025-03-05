using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace IEC60870_5_104_simulator.API
{
    public class Iec104SimulationOptions
    {
        public const string Iec104Simulation = "Iec104Simulation";

        [Range(0, 100000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int CycleTimeMs { get; set; }
        [ValidateObjectMembers]
        public IecConfiguration DataPointConfiguration { get; set; }


        public class IecConfiguration
        {
            [ValidateEnumeratedItems]
            public List<DataPointConfig> Measures { get; set; } = new List<DataPointConfig>();
            [ValidateEnumeratedItems]
            public List<CommandPointConfig> Commands { get; set; } = new List<CommandPointConfig>();
        }
        public class DataPointConfig
        {
            [Range(0, int.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
            public int Ca { get; set; }
            [Range(0, int.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
            public int Oa { get; set; }
            [Range(0, 256, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
            public int TypeId { get; set; }
            [Required(AllowEmptyStrings = false)]
            public string Id { get; set; }
            public SimulationModeConfig Mode { get; set; }

        }
        public enum SimulationModeConfig
        {
            None,
            Cyclic,
            Random
        }
        public class CommandPointConfig
        {
            [Range(0, int.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
            public int Ca { get; set; }
            [Range(0, int.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
            public int Oa { get; set; }
            [Range(45, 87, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
            public int TypeId { get; set; }
            [Required(AllowEmptyStrings = false)]
            public required string Id { get; set; }
            public string ResponseId { get; set; }
        }
    }
}
