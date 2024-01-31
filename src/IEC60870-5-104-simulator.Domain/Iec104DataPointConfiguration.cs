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
        List<Iec104CommandDataPoint> commandDataPoints;
        public Iec104DataPointConfiguration()
        {
            dataPoints = new List<Iec104DataPoint>();
            commandDataPoints = new List<Iec104CommandDataPoint>(); 
            SetupHardcoded();
        }

        private void SetupHardcoded()
        {
            //hardcoded
            dataPoints.Add(new Iec104DataPoint(1, Iec104DataTypes.M_SP_NA_1));
            dataPoints.Add(new Iec104DataPoint(101, Iec104DataTypes.M_SP_NA_1));
            dataPoints.Add(new Iec104DataPoint(3, Iec104DataTypes.M_DP_NA_1));
            Iec104DataPoint steppositionTest = new Iec104DataPoint(9, Iec104DataTypes.M_ST_NA_1);
            dataPoints.Add(steppositionTest);

            var firstcommand = new Iec104CommandDataPoint(10, Iec104DataTypes.C_RC_NA_1);
            firstcommand.AssignResponseDataPoint(steppositionTest);
            commandDataPoints.Add(new Iec104CommandDataPoint(10, Iec104DataTypes.C_RC_NA_1));
        }

        public List<Iec104DataPoint> GetDataPointList()
        {
            return dataPoints;
        }

    }
}
