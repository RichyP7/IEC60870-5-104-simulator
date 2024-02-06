﻿namespace IEC60870_5_104_simulator.Domain.ValueTypes
{
    public abstract record IecValueObject
    {
        public abstract object GetValue();
    }
    public record IecIntValueObject : IecValueObject
    {
        public int Value { get; set; }
        public IecIntValueObject(int value)
        {
            Value = value;
        }

        public override object GetValue()
        {
            return Value;
        }
    }
    public record IecSinglePointValueObject : IecValueObject
    {
        public bool Value { get; set; }
        public IecSinglePointValueObject(bool value)
        {
            Value = value;
        }

        public override object GetValue()
        {
            return Value;
        }
    }
    public record IecDoublePointValueObject : IecValueObject
    {
        public IecDoublePointValue Value { get; set; }
        public IecDoublePointValueObject(IecDoublePointValue value)
        {
            Value = value;
        }

        public override object GetValue()
        {
            return Value;
        }
    }
}