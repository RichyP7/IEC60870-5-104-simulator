using System.ComponentModel.DataAnnotations;
using IEC60870_5_104_simulator.Domain;

namespace IEC60870_5_104_simulator.API.Mapping;

public class Iec104DataPointDto
{
    public string Id { get; set; }
    [Required]
    public int stationaryAddress { get; set; }
    [Required]
    public int objectAddress { get; set; }
    [Required]
    public required Iec104DataTypes Iec104DataType { get; set; }
    public IecValueDto Value { get; set; } = new IecValueDto();

    public SimulationMode Mode { get; set; }
}
public class IecValueDto
{
    public IntValueDto? NumericValue { get; set; }
    public SinglePointValueDto? SinglePointValue { get; set; }
    public DoublePointValueDto? DoublePointValue { get; set; }
    public FloatValueDto? FloatValue { get; set; }
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
    public IecDoublePointValueEnumDto Value { get; set; }
}
public enum IecDoublePointValueEnumDto
{
    INTERMEDIATE,
    OFF,
    ON,
    INDETERMINATE
}

public class FloatValueDto
{
    public float Value { get; set; }
}