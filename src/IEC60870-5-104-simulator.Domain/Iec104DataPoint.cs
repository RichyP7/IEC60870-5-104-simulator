using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IEC60870_5_104_simulator.Domain
{
    public class Iec104DataPoint
    {
        public int ObjectAdress { get;  }
        public Iec104DataTypes Iec104DataTypes { get; }


        public Iec104DataPoint(int objectAdress, Iec104DataTypes iec104DataTypes)
        {
            ObjectAdress = objectAdress;
            Iec104DataTypes = iec104DataTypes;
        }
    }
}
