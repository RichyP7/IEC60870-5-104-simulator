using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IEC60870_5_104_simulator.Domain
{
    public class Iec104DataPointConfig
    {
        public IecAddress Address { get;  }
        public Iec104DataTypes Iec104DataType { get; }


        public Iec104DataPointConfig(IecAddress address, Iec104DataTypes iec104DataTypes)
        {
            Address = address;
            Iec104DataType = iec104DataTypes;
        }
    }
}
