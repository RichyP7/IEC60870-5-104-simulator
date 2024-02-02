using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IEC60870_5_104_simulator.Domain
{
    public  record IecAddress
    {
        public IecAddress(int stationaryAddress, int objectAddress)
        {
            StationaryAddress = stationaryAddress;
            ObjectAddress = objectAddress;
        }

        public int StationaryAddress { get; set; }
        public int ObjectAddress { get; set; }
    }
}
