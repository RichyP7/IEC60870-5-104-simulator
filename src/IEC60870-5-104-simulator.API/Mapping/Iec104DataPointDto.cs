using System.ComponentModel.DataAnnotations;
using IEC60870_5_104_simulator.Domain;

namespace IEC60870_5_104_simulator.API.Mapping;

public class Iec104DataPointDto
{
    public string Id { get; set; }
    [Required]
    public int StationaryAddress { get; set; }
    [Required]
    public int ObjectAddress { get; set; }
    [Required]
    public required Iec104DataTypes Iec104DataType { get; set; }
    public IecValueDto Value { get; set; } = new IecValueDto();

    public SimulationModeDto Mode { get; set; }

    // Simulation realism metadata
    public double? BaseValue { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public double? FluctuationRate { get; set; }
    public string? LinkedPowerPointId { get; set; }
    public float[]? ProfileValues { get; set; }
    public bool Frozen { get; set; }
}
public class IecValueDto
{
    public IntValueDto? NumericValue { get; set; }
    public SinglePointValueDto? SinglePointValue { get; set; }
    public DoublePointValueDto? DoublePointValue { get; set; }
    public FloatValueDto? FloatValue { get; set; }
    public ScaledValueDto? ScaledValue { get; set; }
}
public class IntValueDto
{
    public int Value { get; set; }
}
public class SinglePointValueDto
{
    public bool Value { get; set; }
}
public class DoublePointValueDto
{
    /// <summary>0=INTERMEDIATE, 1=OFF, 2=ON, 3=INDETERMINATE</summary>
    public int Value { get; set; }
}

public class FloatValueDto
{
    public float Value { get; set; }
}
public record ScaledValueDto
{
    public int Value { get; set; }
    public short ShortValue { get; set; }
}
public enum SimulationModeDto
{
    Static,
    Periodic,
    RandomWalk,
    GaussianNoise,
    Solar,
    Wind,
    EnergyCounter,
    CounterOnDemand,
    Profile,
    CommandResponse,
}