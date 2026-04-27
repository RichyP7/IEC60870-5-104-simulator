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
        private DataPointValueService _valueService;

        public DataPointConfigsController(DataPointConfigService dataPointService, DataPointValueService valueService)
        {
            _dataPointService = dataPointService;
            _valueService = valueService;
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
        public Iec104DataPointDto SetSimulationMode([FromRoute] int idStationary, [FromRoute] int idObject, [FromBody] SimulationModeDto simulationMode)
        {
            IecAddress address = new IecAddress(idStationary, idObject);
            var dataPoint = _dataPointService.UpdateSimulationMode(address, (SimulationMode)simulationMode);
            return dataPoint;
        }

        /// <summary>
        /// Unified update: sets value + simulation parameters in one call.
        /// Also sends the new value as a spontaneous IEC-104 message.
        /// </summary>
        [HttpPut("{idStationary}/{idObject}")]
        public async Task<IActionResult> UpdateDataPoint([FromRoute] int idStationary, [FromRoute] int idObject, [FromBody] Iec104DataPointDto dto)
        {
            try
            {
                var address = new IecAddress(idStationary, idObject);
                _dataPointService.UpdateDataPointParams(address, dto);
                await _valueService.SendNewIecValue(dto);
                return Ok(_dataPointService.GetDataPoint(address));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex) when (ex is AutoMapper.AutoMapperMappingException || ex is InvalidCastException)
            {
                return BadRequest("Supplied value is invalid");
            }
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
