﻿using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.Domain
{
    public class Iec104DataPoint
    {
        public string Id { get; set; }
        public IecAddress Address { get; set; }
        public Iec104DataTypes Iec104DataType { get; set; }
        public IecValueObject Value { get; set; }
        public SimulationMode Mode { get; set; }
        public string InitString { get; set; }


        public Iec104DataPoint(IecAddress address, Iec104DataTypes iec104DataType)
        {
            Address = address;
            this.Iec104DataType = iec104DataType;
        }

        public Iec104DataPoint()
        {
        }
    }
    public enum SimulationMode
    {
        None,
        Cyclic,
        CyclicStatic,
        Response
    }
}
