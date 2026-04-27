using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using IEC60870_5_104_simulator.Infrastructure.Interfaces;
using IEC60870_5_104_simulator.Infrastructure.Simulation;
using lib60870.CS101;
using IEC60870_5_104_simulator.Domain.Interfaces;

namespace IEC60870_5_104_simulator.Infrastructure
{
    public class ObjectFactory : IInformationObjectFactory
    {
        private readonly IInformationObjectTemplate _template;
        private readonly IIecValueRepository _repository;
        private readonly Random _random;
        private readonly Dictionary<SimulationMode, ISimulationStrategy> _strategies;

        public ObjectFactory(IInformationObjectTemplate template, IIecValueRepository valueRepository)
        {
            _template = template;
            _repository = valueRepository;
            _random = new Random();
            _strategies = new Dictionary<SimulationMode, ISimulationStrategy>
            {
                [SimulationMode.GaussianNoise] = new GaussianNoiseStrategy(_random),
                [SimulationMode.Solar]         = new SolarStrategy(_random),
                [SimulationMode.Wind]          = new WindStrategy(_random),
                [SimulationMode.RandomWalk]    = new RandomWalkStrategy(_random),
            };
        }

        public InformationObject CreateRandomInformationObject(Iec104DataPoint dp)
        {
            if (_strategies.TryGetValue(dp.Mode, out var strategy))
                return ApplyStrategy(dp, strategy);

            return CreatePurelyRandomObject(dp);
        }

        public InformationObject CreateInformationObjectWithValue(Iec104DataPoint dp, IecValueObject value)
        {
            switch (dp.Iec104DataType)
            {
                case Iec104DataTypes.M_ST_NA_1:
                case Iec104DataTypes.M_ST_TA_1:
                case Iec104DataTypes.M_ST_TB_1:
                    return _template.GetStepposition(dp.Address.ObjectAddress, value, dp.Iec104DataType);
                case Iec104DataTypes.M_SP_NA_1:
                case Iec104DataTypes.M_SP_TA_1:
                case Iec104DataTypes.M_SP_TB_1:
                    return _template.GetSinglePoint(dp.Address.ObjectAddress, value, dp.Iec104DataType);
                case Iec104DataTypes.M_DP_NA_1:
                case Iec104DataTypes.M_DP_TA_1:
                case Iec104DataTypes.M_DP_TB_1:
                    return _template.GetDoublePoint(dp.Address.ObjectAddress, (IecDoublePointValueObject)value, dp.Iec104DataType);
                case Iec104DataTypes.M_ME_NB_1:
                case Iec104DataTypes.M_ME_TB_1:
                case Iec104DataTypes.M_ME_TE_1:
                    return _template.GetMeasuredValueScaled(dp.Address.ObjectAddress, (IecValueScaledObject)value, dp.Iec104DataType);
                case Iec104DataTypes.M_ME_NC_1:
                case Iec104DataTypes.M_ME_TC_1:
                case Iec104DataTypes.M_ME_TF_1:
                    return _template.GetMeasuredValueShort(dp.Address.ObjectAddress, (IecValueFloatObject)value, dp.Iec104DataType);
                case Iec104DataTypes.M_ME_NA_1:
                case Iec104DataTypes.M_ME_TA_1:
                case Iec104DataTypes.M_ME_ND_1:
                    return _template.GetMeasuredValueNormalized(dp.Address.ObjectAddress, (IecValueFloatObject)value, dp.Iec104DataType);
                default:
                    throw new NotImplementedException($"{dp.Iec104DataType} is not implemented");
            }
        }

        // -----------------------------------------------------------------------
        // Strategy dispatch
        // -----------------------------------------------------------------------

        /// <summary>
        /// Applies a simulation strategy to compute a value and wraps it into an InformationObject.
        /// For discrete types (M_SP_*, M_DP_*) under RandomWalk, delegates to random-toggle logic.
        /// For other modes on discrete types, falls back to purely random.
        /// </summary>
        private InformationObject ApplyStrategy(Iec104DataPoint dp, ISimulationStrategy strategy)
        {
            if (!dp.Iec104DataType.IsAnalogOrStep())
            {
                return dp.Mode == SimulationMode.RandomWalk
                    ? CreateRandomDiscreteObject(dp)
                    : CreatePurelyRandomObject(dp);
            }

            double currentValue = GetCurrentNumericValue(dp);
            double raw = strategy.ComputeValue(dp, currentValue);
            double min = dp.Iec104DataType.IsStepPosition() ? dp.MinValue ?? -64  : dp.MinValue ?? double.MinValue;
            double max = dp.Iec104DataType.IsStepPosition() ? dp.MaxValue ?? 63   : dp.MaxValue ?? double.MaxValue;
            double clamped = Math.Clamp(raw, min, max);
            return PersistAndWrapAnalog(dp, clamped);
        }

        // -----------------------------------------------------------------------
        // Value wrapping: persists to repository and builds InformationObject
        // -----------------------------------------------------------------------

        /// <summary>
        /// Persists <paramref name="value"/> to the repository and wraps it in the correct
        /// lib60870 InformationObject for the data point's type.
        /// Handles all analog (M_ME_*) and step-position (M_ST_*) types.
        /// </summary>
        private InformationObject PersistAndWrapAnalog(Iec104DataPoint dp, double value)
        {
            switch (dp.Iec104DataType)
            {
                case Iec104DataTypes.M_ME_NC_1:
                case Iec104DataTypes.M_ME_TC_1:
                case Iec104DataTypes.M_ME_TF_1:
                    var floatVal = new IecValueFloatObject((float)value);
                    _repository.SetObjectValue(dp.Address, floatVal);
                    return _template.GetMeasuredValueShort(dp.Address.ObjectAddress, floatVal, dp.Iec104DataType);

                case Iec104DataTypes.M_ME_NB_1:
                case Iec104DataTypes.M_ME_TB_1:
                case Iec104DataTypes.M_ME_TE_1:
                    var scaledVal = new IecValueScaledObject(new ScaledValueRecord((int)value));
                    _repository.SetObjectValue(dp.Address, scaledVal);
                    return _template.GetMeasuredValueScaled(dp.Address.ObjectAddress, scaledVal, dp.Iec104DataType);

                case Iec104DataTypes.M_ME_NA_1:
                case Iec104DataTypes.M_ME_TA_1:
                case Iec104DataTypes.M_ME_ND_1:
                    var normVal = new IecValueFloatObject((float)value);
                    _repository.SetObjectValue(dp.Address, normVal);
                    return _template.GetMeasuredValueNormalized(dp.Address.ObjectAddress, normVal, dp.Iec104DataType);

                case Iec104DataTypes.M_ST_NA_1:
                case Iec104DataTypes.M_ST_TA_1:
                case Iec104DataTypes.M_ST_TB_1:
                    int stepVal = (int)Math.Round(value);
                    _repository.SetStepValue(dp.Address, stepVal);
                    return _template.GetStepposition(dp.Address.ObjectAddress, new IecIntValueObject(stepVal), dp.Iec104DataType);

                default:
                    return CreatePurelyRandomObject(dp);
            }
        }

        // -----------------------------------------------------------------------
        // Pure random (no simulation mode / fallback)
        // -----------------------------------------------------------------------

        private InformationObject CreatePurelyRandomObject(Iec104DataPoint dp)
        {
            switch (dp.Iec104DataType)
            {
                case Iec104DataTypes.M_ST_NA_1:
                case Iec104DataTypes.M_ST_TA_1:
                case Iec104DataTypes.M_ST_TB_1:
                    int stepValue = CreateRandomStepValue(dp.Address);
                    return _template.GetStepposition(dp.Address.ObjectAddress, new IecIntValueObject(stepValue), dp.Iec104DataType);

                case Iec104DataTypes.M_SP_NA_1:
                case Iec104DataTypes.M_SP_TA_1:
                case Iec104DataTypes.M_SP_TB_1:
                    bool spValue = CreateSinglePointValue(dp.Address);
                    return _template.GetSinglePoint(dp.Address.ObjectAddress, new IecSinglePointValueObject(spValue), dp.Iec104DataType);

                case Iec104DataTypes.M_DP_NA_1:
                case Iec104DataTypes.M_DP_TA_1:
                case Iec104DataTypes.M_DP_TB_1:
                    IecDoublePointValue dpValue = CreateDoublePointValue(dp.Address);
                    return _template.GetDoublePoint(dp.Address.ObjectAddress, new IecDoublePointValueObject(dpValue), dp.Iec104DataType);

                case Iec104DataTypes.M_ME_NB_1:
                case Iec104DataTypes.M_ME_TB_1:
                case Iec104DataTypes.M_ME_TE_1:
                    int scaled = CreateRandomScaledValue(dp.Address);
                    return _template.GetMeasuredValueScaled(dp.Address.ObjectAddress, new IecValueScaledObject(new ScaledValueRecord(scaled)), dp.Iec104DataType);

                case Iec104DataTypes.M_ME_NC_1:
                case Iec104DataTypes.M_ME_TC_1:
                case Iec104DataTypes.M_ME_TF_1:
                    float floatValue = CreateRandomFloat(dp.Address);
                    return _template.GetMeasuredValueShort(dp.Address.ObjectAddress, new IecValueFloatObject(floatValue), dp.Iec104DataType);

                case Iec104DataTypes.M_ME_NA_1:
                case Iec104DataTypes.M_ME_TA_1:
                case Iec104DataTypes.M_ME_ND_1:
                    float normalizedValue = CreateRandomFloat(dp.Address);
                    return _template.GetMeasuredValueNormalized(dp.Address.ObjectAddress, new IecValueFloatObject(normalizedValue), dp.Iec104DataType);

                default:
                    throw new NotImplementedException($"{dp.Iec104DataType} is not implemented");
            }
        }

        /// <summary>RandomWalk for discrete M_SP_* and M_DP_* types: randomly toggles the value.</summary>
        private InformationObject CreateRandomDiscreteObject(Iec104DataPoint dp)
        {
            switch (dp.Iec104DataType)
            {
                case Iec104DataTypes.M_SP_NA_1:
                case Iec104DataTypes.M_SP_TA_1:
                case Iec104DataTypes.M_SP_TB_1:
                    bool spVal = CreateSinglePointValue(dp.Address);
                    return _template.GetSinglePoint(dp.Address.ObjectAddress, new IecSinglePointValueObject(spVal), dp.Iec104DataType);

                case Iec104DataTypes.M_DP_NA_1:
                case Iec104DataTypes.M_DP_TA_1:
                case Iec104DataTypes.M_DP_TB_1:
                    IecDoublePointValue dpVal = CreateDoublePointValue(dp.Address);
                    return _template.GetDoublePoint(dp.Address.ObjectAddress, new IecDoublePointValueObject(dpVal), dp.Iec104DataType);

                default:
                    throw new NotImplementedException($"{dp.Iec104DataType} RandomWalk discrete is not implemented");
            }
        }

        // -----------------------------------------------------------------------
        // Low-level random value generators (persist to repository)
        // -----------------------------------------------------------------------

        private float CreateRandomFloat(IecAddress address)
        {
            float value = NextFloat(_random);
            _repository.SetObjectValue(address, new IecValueFloatObject(value));
            return value;
        }

        private int CreateRandomScaledValue(IecAddress address)
        {
            int value = _random.Next();
            _repository.SetObjectValue(address, new IecIntValueObject(value));
            return value;
        }

        private bool CreateSinglePointValue(IecAddress address)
        {
            bool value = _random.NextDouble() >= 0.5;
            _repository.SetSinglePoint(address, value);
            return value;
        }

        private IecDoublePointValue CreateDoublePointValue(IecAddress address)
        {
            IecDoublePointValue value = _random.NextDouble() >= 0.5 ? IecDoublePointValue.OFF : IecDoublePointValue.ON;
            _repository.SetDoublePoint(address, value);
            return value;
        }

        private int CreateRandomStepValue(IecAddress address)
        {
            int value = _random.Next(-64, 63);
            _repository.SetStepValue(address, value);
            return value;
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private double GetCurrentNumericValue(Iec104DataPoint dp)
        {
            try
            {
                var stored = _repository.GetDataPointValue(dp.Address);
                return stored.Value switch
                {
                    IecValueScaledObject sv => sv.Value.Value,
                    IecValueFloatObject fv  => fv.Value,
                    IecIntValueObject iv    => iv.Value,
                    _                       => dp.BaseValue ?? 0.0
                };
            }
            catch
            {
                return dp.BaseValue ?? 0.0;
            }
        }

        private static float NextFloat(Random random)
        {
            double mantissa = (random.NextDouble() * 2.0) - 1.0;
            double exponent = Math.Pow(2.0, random.Next(-126, 128));
            return (float)(mantissa * exponent);
        }
    }
}