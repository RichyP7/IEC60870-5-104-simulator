using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using static IEC60870_5_104_simulator.API.Iec104SimulationOptions;

namespace IEC60870_5_104_simulator.API.Mapping
{
    /// <summary>
    /// Manual, compile-time-safe mappings between configuration/DTO types and domain types.
    /// Replaces AutoMapper: the mappings are few and simple, so plain static methods are
    /// easier to debug, faster and dependency-free.
    /// </summary>
    public static class IecMappings
    {
        // ------------------------------------------------------------------
        // Configuration -> Domain
        // ------------------------------------------------------------------

        public static Iec104CommandDataPointConfig ToDomain(this CommandPointConfig src) => new()
        {
            Id = src.Id ?? string.Empty,
            Name = src.Name,
            Address = new IecAddress(src.Ca, src.Oa),
            Iec104DataType = (Iec104DataTypes)src.TypeId,
        };

        public static Iec104DataPoint ToDomain(this DataPointConfig src) => new()
        {
            Id = src.Id ?? string.Empty,
            Name = src.Name,
            Address = new IecAddress(src.Ca, src.Oa),
            Iec104DataType = (Iec104DataTypes)src.TypeId,
            InitString = src.InitValue,
            Mode = ToDomain(src.Mode),
            ProfileValues = src.ProfileValues,
            BaseValue = src.BaseValue,
            MinValue = src.MinValue,
            MaxValue = src.MaxValue,
            FluctuationRate = src.FluctuationRate,
            WavePeriodSeconds = src.WavePeriodSeconds,
            LinkedDataPointId = src.LinkedDataPointId,
        };

        public static SimulationMode ToDomain(this SimulationModeConfig config) => config switch
        {
            SimulationModeConfig.GaussianNoise   => SimulationMode.GaussianNoise,
            SimulationModeConfig.PeriodicWave    => SimulationMode.PeriodicWave,
            SimulationModeConfig.EnergyCounter   => SimulationMode.EnergyCounter,
            SimulationModeConfig.CounterOnDemand => SimulationMode.CounterOnDemand,
            SimulationModeConfig.Profile         => SimulationMode.Profile,
            SimulationModeConfig.Periodic        => SimulationMode.Periodic,
            SimulationModeConfig.RandomWalk      => SimulationMode.RandomWalk,
            SimulationModeConfig.CommandResponse => SimulationMode.CommandResponse,
            _                                    => SimulationMode.Static
        };

        // ------------------------------------------------------------------
        // Domain <-> DTO
        // ------------------------------------------------------------------

        public static Iec104DataPointDto ToDto(this Iec104DataPoint src) => new()
        {
            Id = src.Id,
            Name = src.Name,
            StationaryAddress = src.Address.StationaryAddress,
            ObjectAddress = src.Address.ObjectAddress,
            Iec104DataType = src.Iec104DataType,
            Value = src.Value != null ? src.Value.ToDto() : new IecValueDto(),
            Mode = (SimulationModeDto)src.Mode,
            BaseValue = src.BaseValue,
            MinValue = src.MinValue,
            MaxValue = src.MaxValue,
            FluctuationRate = src.FluctuationRate,
            WavePeriodSeconds = src.WavePeriodSeconds,
            LinkedDataPointId = src.LinkedDataPointId,
            ProfileValues = src.ProfileValues,
            Frozen = src.Frozen,
        };

        public static Iec104DataPoint ToDomain(this Iec104DataPointDto src) => new()
        {
            Id = src.Id,
            Name = src.Name,
            Address = new IecAddress(src.StationaryAddress, src.ObjectAddress),
            Iec104DataType = src.Iec104DataType,
            Value = src.Value != null ? src.Value.ToDomain() : null!,
            Mode = (SimulationMode)src.Mode,
            BaseValue = src.BaseValue,
            MinValue = src.MinValue,
            MaxValue = src.MaxValue,
            FluctuationRate = src.FluctuationRate,
            WavePeriodSeconds = src.WavePeriodSeconds,
            LinkedDataPointId = src.LinkedDataPointId,
            ProfileValues = src.ProfileValues,
            Frozen = src.Frozen,
        };

        // ------------------------------------------------------------------
        // Value objects <-> DTO
        // ------------------------------------------------------------------

        public static IecValueObject ToDomain(this IecValueDto source)
        {
            if (source.NumericValue != null)
                return new IecIntValueObject(source.NumericValue.Value);
            if (source.SinglePointValue != null)
                return new IecSinglePointValueObject(source.SinglePointValue.Value);
            if (source.DoublePointValue != null)
                return new IecDoublePointValueObject((IecDoublePointValue)source.DoublePointValue.Value);
            if (source.FloatValue != null)
                return new IecValueFloatObject(source.FloatValue.Value);
            if (source.ScaledValue != null)
                return new IecValueScaledObject(new ScaledValueRecord(source.ScaledValue.Value));
            throw new InvalidCastException("Invalid dto cast");
        }

        public static IecValueDto ToDto(this IecValueObject source) => source switch
        {
            IecIntValueObject intCasted          => new IecValueDto { NumericValue = new IntValueDto { Value = intCasted.Value } },
            IecSinglePointValueObject spCasted   => new IecValueDto { SinglePointValue = new SinglePointValueDto { Value = spCasted.Value } },
            IecDoublePointValueObject dpCasted   => new IecValueDto { DoublePointValue = new DoublePointValueDto { Value = (int)dpCasted.Value } },
            IecValueFloatObject castedFloat      => new IecValueDto { FloatValue = new FloatValueDto { Value = castedFloat.Value } },
            IecValueScaledObject casted          => new IecValueDto { ScaledValue = new ScaledValueDto { Value = casted.Value.Value, ShortValue = casted.Value.ShortValue } },
            _                                    => throw new InvalidCastException("Invalid dto cast")
        };
    }
}
