using IEC60870_5_104_simulator.API.Mapping;
using IEC60870_5_104_simulator.API.Services;
using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IEC60870_5_104_simulator.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataPointConfigsController : ControllerBase
    {
        private DataPointConfigService _dataPointService;
        public DataPointConfigsController(DataPointConfigService dataPointService)
        {
            _dataPointService = dataPointService;
        }
        
        // GET: api/<DataPointsController>
        [HttpGet]
        public IEnumerable<Iec104DataPointDto> Get()
        {
            var data = _dataPointService.GetAllDataPoints();
            
            return data;
        }
        
        [HttpGet("{idStationary}/{idObject}")]
        public Iec104DataPointDto Get([FromRoute ]int idStationary, [FromRoute] int idObject)
        {
            IecAddress address = new IecAddress(idStationary, idObject);
            var dataPoint = _dataPointService.GetDataPoint(address);
            return dataPoint;
        }

        [HttpPut("{idStationary}/{idObject}/simulation-mode")]
        public Iec104DataPointDto SetSimulationMode([FromRoute] int idStationary, [FromRoute] int idObject, [FromBody] SimulationMode simulationMode)
        {
            IecAddress address = new IecAddress(idStationary, idObject);
            var dataPoint = _dataPointService.UpdateSimulationMode(address, simulationMode);
            return dataPoint;
        }
        
        [HttpPost]
        public Iec104DataPoint Post([FromBody] Iec104DataPointDto dataPoint)
        {
            var createdDataPoint = _dataPointService.CreateDataPoint(dataPoint) ;
            return createdDataPoint;
        }
        
        
        [HttpDelete("{idStationary}/{idObject}")]
        public IActionResult Delete([FromRoute ]int idStationary, [FromRoute] int idObject)
        {
            var deleteSuccess = _dataPointService.DeleteDataPoint(idStationary, idObject);
            if (deleteSuccess)
            {
                return NoContent();
            }
            else return NotFound();
        }
    }
}
