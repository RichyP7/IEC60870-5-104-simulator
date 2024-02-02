using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IEC60870_5_104_simulator.Domain
{
    public class Iec104DataPointConfig
    {
        public string Id { get; set; }
        public IecAddress Address { get; set; }
        public Iec104DataTypes Iec104DataType { get; set; }


        public Iec104DataPointConfig(IecAddress address, Iec104DataTypes iec104DataType)
        {
            Address = address;
            this.Iec104DataType = iec104DataType;
        }

        public Iec104DataPointConfig()
        {
        }
    }
}
