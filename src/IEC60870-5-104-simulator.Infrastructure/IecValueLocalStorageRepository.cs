using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        public bool SetSinglePoint(IecAddress address,bool value)
        {
            if (StoredDataPoints.TryGetValue(address, out Iec104DataPoint test))
            {
                test.Value = new IecSinglePointValueObject(value);
            }
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

        public IecDoublePointValue SetDoublePoint(IecAddress address, IecDoublePointValue value)
        {
            if (StoredDataPoints.TryGetValue(address, out Iec104DataPoint test))
            {
                test.Value = new IecDoublePointValueObject(value);
            }
            throw new KeyNotFoundException($"invalidkey for Ca: {address.StationaryAddress} Oa:{address.ObjectAddress} ");
        }
    }
    
}
