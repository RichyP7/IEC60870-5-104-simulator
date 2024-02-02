using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IEC60870_5_104_simulator.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataPointConfigsController : ControllerBase
    {
        // GET: api/<DataPointsController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<DataPointsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
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
