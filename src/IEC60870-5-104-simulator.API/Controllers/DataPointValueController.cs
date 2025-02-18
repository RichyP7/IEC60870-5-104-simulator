using IEC60870_5_104_simulator.Domain.ValueTypes;
using IEC60870_5_104_simulator.Infrastructure.DataPointsService;
using IEC60870_5_104_simulator.Infrastructure.Dto;
using Microsoft.AspNetCore.Mvc;

namespace IEC60870_5_104_simulator.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DataPointValueController(ValueService valueService) : ControllerBase
{
    [HttpPut("{idStationary}/{idObject}")]
    public Iec104DataPointDto SetSimulationMode([FromRoute] int idStationary, [FromRoute] int idObject, [FromBody] String newValue)
    {
        IecAddress address = new IecAddress(idStationary, idObject);
        var dataPoint = valueService.UpdateDataPointValue(address, newValue);
        return dataPoint;
    }
}