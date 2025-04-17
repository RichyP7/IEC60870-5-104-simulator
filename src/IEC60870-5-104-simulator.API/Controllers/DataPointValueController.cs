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
    [HttpGet("{idStationary}/{idObject}")]
    public async Task<ActionResult<Iec104DataPointDto>> Get([FromRoute] int idStationary, [FromRoute] int idObject)
    {
        try
        {
            var result = await dpService.GetCurrentValue(new IecAddress(idStationary, idObject));
            return new OkObjectResult(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex) when (ex is AutoMapperMappingException || ex is InvalidCastException)
        {
            return BadRequest("Supplied value to send is invalid");
        }
    }
}