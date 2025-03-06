using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.Domain
{
    public class Iec104CommandDataPointConfig
    {
        public string Id { get; set; }
        public IecAddress Address { get; set; }
        public Iec104DataTypes Iec104DataType { get; set; }
        public Iec104DataPoint SimulatedDataPoint { get; set; }


        public Iec104CommandDataPointConfig(IecAddress adress, Iec104DataTypes iec104DataTypes)
        {
            Address = adress;
            Iec104DataType = iec104DataTypes;
        }

        public Iec104CommandDataPointConfig()
        {
        }

        public void AssignResponseDataPoint(Iec104DataPoint dataPoint) 
        {
            SimulatedDataPoint = dataPoint ?? throw new ArgumentNullException(nameof(dataPoint));
        }
    }
}
