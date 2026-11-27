using System.Globalization;
using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.Domain.Service
{
    /// <summary>
    /// Turns a configured init string into a typed value, picking by datapoint type.
    /// </summary>
    /// <remarks>
    /// Semantics deliberately match the previous StorageDataPoints.AddDataPoint exactly: parse
    /// failure falls back to a zero value and step positions are clamped to the IEC-104 range.
    /// This module exists so the behaviour has a name; unifying the four divergent parsers in
    /// the codebase is a separate change.
    /// </remarks>
    internal static class InitValueParser
    {
        public static IecValueObject Parse(string? initString, Iec104DataTypes dataType)
        {
            return dataType switch
            {
                Iec104DataTypes.M_ST_NA_1 or Iec104DataTypes.M_ST_TA_1 or Iec104DataTypes.M_ST_TB_1
                    => StepValue(initString),
                Iec104DataTypes.M_SP_NA_1 or Iec104DataTypes.M_SP_TA_1 or Iec104DataTypes.M_SP_TB_1
                    => SinglePoint(initString),
                Iec104DataTypes.M_DP_NA_1 or Iec104DataTypes.M_DP_TA_1 or Iec104DataTypes.M_DP_TB_1
                    => DoublePoint(initString),
                Iec104DataTypes.M_ME_NB_1 or Iec104DataTypes.M_ME_TB_1 or Iec104DataTypes.M_ME_TE_1
                    => ScaledValue(initString),
                Iec104DataTypes.M_ME_NC_1 or Iec104DataTypes.M_ME_TC_1 or Iec104DataTypes.M_ME_TF_1
                    => FloatValue(initString),
                Iec104DataTypes.M_ME_NA_1 or Iec104DataTypes.M_ME_TA_1 or Iec104DataTypes.M_ME_ND_1
                    => FloatValue(initString),
                _ => throw new NotImplementedException($"{dataType} is not implemented")
            };
        }

        private static IecSinglePointValueObject SinglePoint(string? initString)
        {
            return !string.IsNullOrEmpty(initString) && bool.TryParse(initString, out bool value)
                ? new IecSinglePointValueObject(value)
                : new IecSinglePointValueObject(false);
        }

        private static IecDoublePointValueObject DoublePoint(string? initString)
        {
            return !string.IsNullOrEmpty(initString) && Enum.TryParse(initString, out IecDoublePointValue value)
                ? new IecDoublePointValueObject(value)
                : new IecDoublePointValueObject(IecDoublePointValue.OFF);
        }

        private static IecValueScaledObject ScaledValue(string? initString)
        {
            return !string.IsNullOrEmpty(initString) && int.TryParse(initString, out int value)
                ? new IecValueScaledObject(new ScaledValueRecord(value))
                : new IecValueScaledObject(new ScaledValueRecord(0));
        }

        private static IecValueFloatObject FloatValue(string? initString)
        {
            return !string.IsNullOrEmpty(initString) &&
                   float.TryParse(initString, CultureInfo.InvariantCulture, out float value)
                ? new IecValueFloatObject(value)
                : new IecValueFloatObject(0);
        }

        private static IecIntValueObject StepValue(string? initString)
        {
            if (string.IsNullOrEmpty(initString) || !int.TryParse(initString, out int parsed))
                return new IecIntValueObject(0);

            return new IecIntValueObject(Math.Clamp(
                parsed,
                StepPositionConst.STEP_POSITION_MIN,
                StepPositionConst.STEP_POSITION_MAX));
        }
    }
}
