using IEC60870_5_104_simulator.Service;
using Microsoft.AspNetCore.Mvc;

namespace IEC60870_5_104_simulator.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SimulationEngineStateController : ControllerBase
    {

        private readonly ILogger<SimulationEngineStateController> _logger;

        public SimulationEngineStateController(ILogger<SimulationEngineStateController> logger, SimulationEngine simulationEngine)
        {
            _logger = logger;
            this.simulationEngine = simulationEngine;
        }

        public SimulationEngine simulationEngine { get; }

        [HttpGet]
        public IActionResult Get()
        {
            return new OkResult();
        }
        [HttpPost(Name = "commands")]
        public async Task<IActionResult> EngineCommand(ENGINE_COMMAND command, CancellationToken cs)
        {
            try
            {
                if (command.Equals(ENGINE_COMMAND.Start))
                {
                    await simulationEngine.StartAsync(cs);
                }
                else if (command.Equals(ENGINE_COMMAND.Stop))
                {
                    CancellationToken cancel = new CancellationToken(true);
                    await simulationEngine.StopAsync(cancel);
                }
                else
                    return new BadRequestObjectResult("Supply valid command");
                
                return new OkObjectResult("started");
            }        
            catch (TaskCanceledException ex) 
            {
                throw new Exception("Task was cancelled", ex);
            }
        }
        public enum ENGINE_COMMAND
        {
            Start,
            Stop
        }

    }
}