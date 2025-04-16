using AutoMapper;
using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.API.Mapping
{
    internal class ValueDtoMapper : ITypeConverter<IecValueObject, IecValueDto>
    {
        public IecValueDto Convert(IecValueObject source, IecValueDto destination, ResolutionContext context)
        {
            if (source is IecIntValueObject intCasted)
                return new IecValueDto { NumericValue = new IntValueDto() { Value = intCasted.Value } };
            else if (source is IecSinglePointValueObject spCasted)
                return new IecValueDto { SinglePointValue = new SinglePointValueDto() { Value = spCasted.Value} };
            else if (source is IecDoublePointValueObject dpCasted)
                return new IecValueDto { DoublePointValue = new DoublePointValueDto() { Value = (IecDoublePointValueEnumDto)dpCasted.Value } };
            else if (source is IecValueFloatObject castedFloat)
                return new IecValueDto { FloatValue = new FloatValueDto() { Value = castedFloat.Value } };
            else if (source is IecValueScaledObject casted)
                return new IecValueDto { ScaledValue = new ScaledValueDto() { Value = casted.Value.Value, ShortValue = casted.Value.ShortValue } };
            else
                throw new InvalidCastException("Invalid dto cast");
        }
    }
}