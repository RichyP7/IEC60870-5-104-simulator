using AutoMapper;
using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.API.Mapping
{
    internal class DtoValueMapper : ITypeConverter<IecValueDto, IecValueObject>
    {
        public IecValueObject Convert(IecValueDto source, IecValueObject destination, ResolutionContext context)
        {
            if (source.NumericValue != null)
                return new IecIntValueObject(source.NumericValue.Value);
            else if (source.SinglePointValue != null)
                return new IecSinglePointValueObject(source.SinglePointValue.Value);
            else if (source.DoublePointValue != null)
                return new IecDoublePointValueObject((IecDoublePointValue)source.DoublePointValue.Value);
            else if (source.FloatValue != null)
                return new IecValueFloatObject(source.FloatValue.Value);
            else if (source.ScaledValue != null)
                return new IecValueScaledObject(new ScaledValueRecord(source.ScaledValue.Value));
            else
                throw new InvalidCastException("Invalid dto cast");
        }
    }
}