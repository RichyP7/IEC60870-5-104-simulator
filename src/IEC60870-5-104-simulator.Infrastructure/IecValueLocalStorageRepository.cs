using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using lib60870.CS101;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IEC60870_5_104_simulator.Infrastructure
{
    internal class IecValueLocalStorageRepository : IIecValueLocalStorageRepository
    {
        public ConcurrentDictionary<IecAddress, Iec104DataPoint> StoredDataPoints;
        public IecValueLocalStorageRepository()
        {
            this.StoredDataPoints = new();
        }

        public int GetStepValue(IecAddress address)
        {
            if (StoredDataPoints.TryGetValue(address, out Iec104DataPoint test))
            {
                var ret = test.Value as IecIntValueObject;
                return ret.Value;
            }
            throw new KeyNotFoundException($"invalidkey for Ca: {address.StationaryAddress} Oa:{address.ObjectAddress} ");
        }
        public void SetStepValue(IecAddress address, int value)
        {
            if (StoredDataPoints.TryGetValue(address, out Iec104DataPoint test))
            {
                var ret = test.Value as IecIntValueObject;
                test.Value = new IecIntValueObject(value);
            }
            else
                throw new KeyNotFoundException($"invalidkey for Ca: {address.StationaryAddress} Oa:{address.ObjectAddress} ");
        }

        public bool GetSinglePoint(IecAddress address)
        {
            if (StoredDataPoints.TryGetValue(address, out Iec104DataPoint test))
            {
                var ret = test.Value as IecSinglePointValueObject;
                return ret?.Value ?? throw new InvalidCastException("boolean cast");
            }
            throw new KeyNotFoundException($"invalidkey for Ca: {address.StationaryAddress} Oa:{address.ObjectAddress} ");
        }
        public void SetSinglePoint(IecAddress address,bool value)
        {
            if (StoredDataPoints.TryGetValue(address, out Iec104DataPoint test))
            {
                test.Value = new IecSinglePointValueObject(value);
            }
            else
                throw new KeyNotFoundException($"invalidkey for Ca: {address.StationaryAddress} Oa:{address.ObjectAddress} ");
        }

        public IecDoublePointValue GetDoublePoint(IecAddress address)
        {
            if (StoredDataPoints.TryGetValue(address, out Iec104DataPoint test))
            {
                var ret = test.Value as IecDoublePointValueObject;
                return ret?.Value ?? throw new InvalidCastException($"double cast: {address.StationaryAddress} Oa:{address.ObjectAddress}");
            }
            throw new KeyNotFoundException($"invalidkey for Ca: {address.StationaryAddress} Oa:{address.ObjectAddress} ");
        }

        public void SetDoublePoint(IecAddress address, IecDoublePointValue value)
        {
            if (StoredDataPoints.TryGetValue(address, out Iec104DataPoint test))
            {
                test.Value = new IecDoublePointValueObject(value);
            }
            else
                throw new KeyNotFoundException($"invalidkey for Ca: {address.StationaryAddress} Oa:{address.ObjectAddress} ");
        }

        public void AddDataPoint(IecAddress address, Iec104DataPoint newdatapoint)
        {
            if(StoredDataPoints.TryAdd(address, newdatapoint))
            {
                switch (newdatapoint.Iec104DataType)
                {
                    case Iec104DataTypes.M_ST_NA_1:
                    case Iec104DataTypes.M_ST_TA_1:
                    case Iec104DataTypes.M_ST_TB_1:
                        newdatapoint.Value = new IecIntValueObject(0);
                        break;
                    case Iec104DataTypes.M_SP_NA_1:
                    case Iec104DataTypes.M_SP_TA_1:
                    case Iec104DataTypes.M_SP_TB_1:
                        newdatapoint.Value = new IecSinglePointValueObject(false);
                        break;
                    case Iec104DataTypes.M_DP_NA_1:
                    case Iec104DataTypes.M_DP_TA_1:
                    case Iec104DataTypes.M_DP_TB_1:
                        newdatapoint.Value = new IecDoublePointValueObject( IecDoublePointValue.OFF);
                        break;
                    case Iec104DataTypes.M_ME_NB_1:
                    case Iec104DataTypes.M_ME_TB_1:
                    case Iec104DataTypes.M_ME_TE_1:
                        newdatapoint.Value = new IecValueScaledObject(new ScaledValueRecord(0));
                        break;
                    case Iec104DataTypes.M_ME_NC_1:
                    case Iec104DataTypes.M_ME_TC_1:
                    case Iec104DataTypes.M_ME_TF_1:
                        newdatapoint.Value = new IecValueShortObject(0.0f);
                        break;
                    default:
                        throw new NotImplementedException($"{newdatapoint.Iec104DataType} is not implemented");
                }
            }
        }
    }
    
}
