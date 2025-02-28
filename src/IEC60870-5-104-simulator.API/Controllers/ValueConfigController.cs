using IEC60870_5_104_simulator.Domain.ValueTypes;
using IEC60870_5_104_simulator.Infrastructure.DataPointsService;
using Microsoft.AspNetCore.Mvc;

namespace IEC60870_5_104_simulator.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ValueConfigController(ValueService valueService) : ControllerBase
{
    // GET: api/<ValueConfigController>
    [HttpGet("{idStationary}/{idObject}")]
    public IActionResult ToggleDigitalPoint([FromRoute ]int idStationary, [FromRoute] int idObject)
    {
        try
        {
            valueService.ToggleDigitalPointValue(new IecAddress(idStationary, idObject));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
            
        return StatusCode(200);
    }
}