namespace IEC60870_5_104_simulator.Domain;

/// <summary>
/// Extension methods that classify <see cref="Iec104DataTypes"/> enum values into semantic groups.
/// Centralises type membership so every layer (mapping, simulation, scenario) uses one definition.
/// </summary>
public static class Iec104DataTypeExtensions
{
    // -----------------------------------------------------------------------
    // Granular groups (one IEC-104 message type family per method)
    // -----------------------------------------------------------------------

    /// <summary>M_ST_NA_1 / M_ST_TA_1 / M_ST_TB_1 — step position measurements.</summary>
    public static bool IsStepPosition(this Iec104DataTypes t) =>
        t is Iec104DataTypes.M_ST_NA_1 or Iec104DataTypes.M_ST_TA_1 or Iec104DataTypes.M_ST_TB_1;

    /// <summary>M_ME_NB_1 / M_ME_TB_1 / M_ME_TE_1 — scaled (16-bit integer) measurements.</summary>
    public static bool IsScaledMeasurement(this Iec104DataTypes t) =>
        t is Iec104DataTypes.M_ME_NB_1 or Iec104DataTypes.M_ME_TB_1 or Iec104DataTypes.M_ME_TE_1;

    /// <summary>M_ME_NC_1 / M_ME_TC_1 / M_ME_TF_1 — short float measurements.</summary>
    public static bool IsShortFloatMeasurement(this Iec104DataTypes t) =>
        t is Iec104DataTypes.M_ME_NC_1 or Iec104DataTypes.M_ME_TC_1 or Iec104DataTypes.M_ME_TF_1;

    /// <summary>M_ME_NA_1 / M_ME_TA_1 / M_ME_ND_1 — normalised float measurements.</summary>
    public static bool IsNormalizedMeasurement(this Iec104DataTypes t) =>
        t is Iec104DataTypes.M_ME_NA_1 or Iec104DataTypes.M_ME_TA_1 or Iec104DataTypes.M_ME_ND_1;

    /// <summary>M_SP_NA_1 / M_SP_TA_1 / M_SP_TB_1 — single-point information.</summary>
    public static bool IsSinglePoint(this Iec104DataTypes t) =>
        t is Iec104DataTypes.M_SP_NA_1 or Iec104DataTypes.M_SP_TA_1 or Iec104DataTypes.M_SP_TB_1;

    /// <summary>M_DP_NA_1 / M_DP_TA_1 / M_DP_TB_1 — double-point information.</summary>
    public static bool IsDoublePoint(this Iec104DataTypes t) =>
        t is Iec104DataTypes.M_DP_NA_1 or Iec104DataTypes.M_DP_TA_1 or Iec104DataTypes.M_DP_TB_1;

    // -----------------------------------------------------------------------
    // Composite helpers (used where multiple granular groups share behaviour)
    // -----------------------------------------------------------------------

    /// <summary>
    /// True when the stored value is an integer — covers step positions (M_ST_*)
    /// and scaled measurements (M_ME_NB/TB/TE).
    /// </summary>
    public static bool IsIntegerValue(this Iec104DataTypes t) =>
        t.IsStepPosition() || t.IsScaledMeasurement();

    /// <summary>
    /// True when the stored value is a float — covers short float (M_ME_NC/TC/TF)
    /// and normalised measurements (M_ME_NA/TA/ND).
    /// </summary>
    public static bool IsFloatValue(this Iec104DataTypes t) =>
        t.IsShortFloatMeasurement() || t.IsNormalizedMeasurement();

    /// <summary>
    /// True for all analog measurement and step-position types (M_ME_* and M_ST_*).
    /// These are the types for which a numeric simulation strategy is meaningful.
    /// </summary>
    public static bool IsAnalogOrStep(this Iec104DataTypes t) =>
        t.IsStepPosition() || t.IsScaledMeasurement() ||
        t.IsShortFloatMeasurement() || t.IsNormalizedMeasurement();
}
