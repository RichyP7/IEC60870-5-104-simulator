using IEC60870_5_104_simulator.Domain;

namespace IEC60870_5_104_simulator.API
{
    public class Iec104DataConfig
    {
        public Iec104DataConfig()
        {

        }
        public List<Iec104DataPoint> GetConfig() 
        { 
            return new List<Iec104DataPoint>()
            {
                new Iec104DataPoint("51",Iec104DataTypes.M_SP_NA_1),
                Iec104DataTypes.M_SP_TA_1,
                Iec104DataTypes.M_DP_NA_1,
                Iec104DataTypes.M_ME_NA_1
            };
        }
    }
}
