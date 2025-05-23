﻿using System.Collections.Concurrent;
using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.Domain.Interfaces
{
    public interface IIecValueRepository
    {
        bool GetSinglePoint(IecAddress address);
        void SetSinglePoint(IecAddress address, bool value);
        IecDoublePointValue GetDoublePoint(IecAddress address);
        void SetDoublePoint(IecAddress address, IecDoublePointValue value);
        void SetObjectValue(IecAddress address, IecValueObject value);
        void SetSimulationMode(IecAddress address, SimulationMode mode);
        int GetStepValue(IecAddress address);
        void SetStepValue(IecAddress address, int value);
        void AddDataPoint(IecAddress address, Iec104DataPoint newdatapoint);
        IEnumerable<Iec104DataPoint> GetAllDataPoints();
        Iec104DataPoint GetDataPointValue(IecAddress address);
        void DeleteDataPoint(IecAddress address);
    }
}