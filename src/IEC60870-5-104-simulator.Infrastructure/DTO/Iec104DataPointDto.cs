﻿using System.ComponentModel.DataAnnotations;
using IEC60870_5_104_simulator.Domain;

namespace IEC60870_5_104_simulator.Infrastructure.Dto;

public class Iec104DataPointDto
{
    public string Id { get; set; }
    [Required]
    public int stationaryAddress { get; set; }
    [Required]
    public int objectAddress { get; set; }
    public Iec104DataTypes Iec104DataType { get; set; }
    public String Value { get; set; }
    public SimulationMode Mode { get; set; }
}