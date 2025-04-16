using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using System.Collections.Concurrent;
using System.Data;

namespace IEC60870_5_104_simulator.Infrastructure
{
    internal class IecValueLocalStorageRepository : IIecValueRepository
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

        public void DeleteDataPoint(IecAddress address)
        {
            if (StoredDataPoints.TryGetValue(address, out Iec104DataPoint foundValue))
            {
                StoredDataPoints.Remove(address, out Iec104DataPoint removedValue);
                if (removedValue != null) return;
            }
            throw new KeyNotFoundException($"invalidkey for Ca: {address.StationaryAddress} Oa:{address.ObjectAddress} ");
        }

        public Iec104DataPoint GetDataPointValue(IecAddress address)
        {
            var found = StoredDataPoints.TryGetValue(address, out var value);
            if (!found || value == null) throw new KeyNotFoundException("DataPoint not found for id" + address);
            return value;
        }

        public void SetSimulationMode(IecAddress address, SimulationMode mode)
        {
            if (StoredDataPoints.TryGetValue(address, out Iec104DataPoint? test))
            {
                test.Mode = mode;
            }
            else
                throw new KeyNotFoundException($"invalidkey for Ca: {address.StationaryAddress} Oa:{address.ObjectAddress} ");
        }

        public void SetSinglePoint(IecAddress address, bool value)
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

        public void SetObjectValue(IecAddress address, IecValueObject value)
        {
            if (StoredDataPoints.TryGetValue(address, out Iec104DataPoint test))
            {
                test.Value = value;
            }
            else
                throw new KeyNotFoundException($"invalidkey for Ca: {address.StationaryAddress} Oa:{address.ObjectAddress} ");
        }
        //Todo: InitValues for non dp,sp
        public void AddDataPoint(IecAddress address, Iec104DataPoint newdatapoint)
        {
            var hasExistingValue = newdatapoint.Value != null!;
            if (StoredDataPoints.TryAdd(address, newdatapoint))
            {
                if (hasExistingValue) return;
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
                        newdatapoint.Value = SetSinglePoint(newdatapoint.InitString);
                        break;
                    case Iec104DataTypes.M_DP_NA_1:
                    case Iec104DataTypes.M_DP_TA_1:
                    case Iec104DataTypes.M_DP_TB_1:
                        newdatapoint.Value = SetDoublePoint(newdatapoint.InitString);
                        break;
                    case Iec104DataTypes.M_ME_NB_1:
                    case Iec104DataTypes.M_ME_TB_1:
                    case Iec104DataTypes.M_ME_TE_1:
                        newdatapoint.Value = new IecValueScaledObject(new ScaledValueRecord(0));
                        break;
                    case Iec104DataTypes.M_ME_NC_1:
                    case Iec104DataTypes.M_ME_TC_1:
                    case Iec104DataTypes.M_ME_TF_1:
                        newdatapoint.Value = new IecValueFloatObject(0.0f);
                        break;
                    case Iec104DataTypes.M_ME_NA_1:
                    case Iec104DataTypes.M_ME_TA_1:
                    case Iec104DataTypes.M_ME_ND_1:
                        newdatapoint.Value = new IecValueFloatObject(0.0f);
                        break;
                    default:
                        throw new NotImplementedException($"{newdatapoint.Iec104DataType} is not implemented");
                }
            }
        }

        private static IecSinglePointValueObject SetSinglePoint(string initString)
        {
            return !String.IsNullOrEmpty(initString) && (Boolean.TryParse(initString, out bool booleanValue))
                ? new IecSinglePointValueObject(booleanValue)
                : new IecSinglePointValueObject(false);
        }

        private static IecDoublePointValueObject SetDoublePoint(string initstring)
        {
            return !String.IsNullOrEmpty(initstring) && (Enum.TryParse(initstring, out IecDoublePointValue doubleValue))
                ? new IecDoublePointValueObject(doubleValue)
                : new IecDoublePointValueObject(IecDoublePointValue.OFF);
        }

        public IEnumerable<Iec104DataPoint> GetAllDataPoints()
        {
            return StoredDataPoints.Values.AsEnumerable();
        }
    }

}
