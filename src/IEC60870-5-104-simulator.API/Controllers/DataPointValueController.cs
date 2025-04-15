using IEC60870_5_104_simulator.API.Mapping;
using IEC60870_5_104_simulator.API.Services;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using Microsoft.AspNetCore.Mvc;

namespace IEC60870_5_104_simulator.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DataPointValuesController : ControllerBase
{
    private readonly DataPointValueService dpService;

    public DataPointValuesController(DataPointValueService service)
    {
        this.dpService = service;
    }

    [HttpPut("{idStationary}/{idObject}/mode")]
    public Iec104DataPointDto SetSimulationMode([FromRoute] int idStationary, [FromRoute] int idObject, [FromBody] String newValue)
    {
        IecAddress address = new IecAddress(idStationary, idObject);
        //var dataPoint = valueService.UpdateDataPointValue(address, newValue);
        return new Iec104DataPointDto() { Iec104DataType = Domain.Iec104DataTypes.ASDU_TYPE_114_119};
    }

    [HttpGet("{idStationary}/{idObject}/toggle")]
    public IActionResult ToggleDigitalPoint([FromRoute] int idStationary, [FromRoute] int idObject)
    {
        try
        {
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        return StatusCode(200);
    }
    [HttpPost("{idStationary}/{idObject}")]
    public IActionResult CreateNewIecValue([FromRoute] int idStationary, [FromRoute] int idObject, Iec104DataPointDto dto)
    {
        try
        {
            dpService.SendNewIecValue(dto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        return Ok();
    }
}