using AutoMapper;
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

    [HttpPost("{idStationary}/{idObject}")]
    public async Task<IActionResult> CreateNewIecValue([FromRoute] int idStationary, [FromRoute] int idObject, Iec104DataPointDto dto)
    {
        try
        {
            await dpService.SendNewIecValue(dto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex) when (ex is AutoMapperMappingException || ex is InvalidCastException)
        {
            return BadRequest("Supplied value to send is invalid");
        }
        return Ok();
    }
}