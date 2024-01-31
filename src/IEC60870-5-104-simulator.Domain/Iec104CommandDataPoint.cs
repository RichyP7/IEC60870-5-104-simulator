using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IEC60870_5_104_simulator.Domain
{
    internal class Iec104CommandDataPoint
    {
        public int ObjectAdress { get; }
        public Iec104DataTypes Iec104DataTypes { get; }
        public Iec104DataPoint SimulatedDataPoint { get; private set; }


        public Iec104CommandDataPoint(int objectAdress, Iec104DataTypes iec104DataTypes)
        {
            ObjectAdress = objectAdress;
            Iec104DataTypes = iec104DataTypes;
        }
        public void AssignResponseDataPoint(Iec104DataPoint dataPoint) 
        {
            SimulatedDataPoint = dataPoint ?? throw new ArgumentNullException(nameof(dataPoint));
        }
    }
}
