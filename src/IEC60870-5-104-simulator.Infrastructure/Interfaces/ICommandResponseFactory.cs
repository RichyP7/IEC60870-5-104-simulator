using IEC60870_5_104_simulator.Domain;
using lib60870.CS101;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IEC60870_5_104_simulator.Infrastructure.Interfaces
{
    public interface ICommandResponseFactory
    {
        InformationObject GetResponseInformationObject(Iec104CommandDataPointConfig datapoint, InformationObject sentCommand);
    }
}
