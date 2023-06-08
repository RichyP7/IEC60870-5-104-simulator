using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IEC60870_5_104_simulator.Domain
{
    public class Iec104DataPointConfiguration
    {
        List<Iec104DataPoint> dataPoints;
        public Iec104DataPointConfiguration()
        {
            dataPoints = new List<Iec104DataPoint>();
            SetupHardcoded();
        }

        private void SetupHardcoded()
        {
            //hardcoded
            dataPoints.Add(new Iec104DataPoint(1, Iec104DataTypes.M_SP_NA_1));
            dataPoints.Add(new Iec104DataPoint(101, Iec104DataTypes.M_SP_NA_1));
            dataPoints.Add(new Iec104DataPoint(3, Iec104DataTypes.M_DP_NA_1));
            dataPoints.Add(new Iec104DataPoint(9, Iec104DataTypes.M_ME_NA_1));
        }

        public List<Iec104DataPoint> GetDataPointList()
        {
            return dataPoints;
        }

    }
}
