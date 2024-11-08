using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using IEC60870_5_104_simulator.Infrastructure.DataPointsService;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IEC60870_5_104_simulator.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataPointConfigsController : ControllerBase
    {
        private DataPointService _dataPointService;
        public DataPointConfigsController(DataPointService dataPointService)
        {
            _dataPointService = dataPointService;
        }
        
        // GET: api/<DataPointsController>
        [HttpGet]
        public IEnumerable<Iec104DataPoint> Get()
        {
            var data = _dataPointService.GetAllDataPoints();
            
            return data;
        }

        // GET api/<DataPointsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            //IecAddress address = new IecAddress(id);
            //var point = _dataPointService.GetDataPoint(id);
            return "";
        }

        // POST api/<DataPointsController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<DataPointsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<DataPointsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
