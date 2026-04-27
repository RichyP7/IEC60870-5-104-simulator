using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Service;
using Microsoft.AspNetCore.Mvc;

namespace IEC60870_5_104_simulator.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScenariosController : ControllerBase
    {
        private readonly IScenarioService _scenarioService;
        private readonly ILogger<ScenariosController> _logger;

        public ScenariosController(IScenarioService scenarioService, ILogger<ScenariosController> logger)
        {
            _scenarioService = scenarioService;
            _logger = logger;
        }

        /// <summary>Returns all available scenarios and their current execution state.</summary>
        [HttpGet]
        public IActionResult GetScenarios()
        {
            var states = _scenarioService.GetStates().Select(s => new
            {
                name = s.Name,
                status = s.Status.ToString(),
                remainingMs = s.RemainingMs
            });
            return Ok(states);
        }

        /// <summary>Triggers the named scenario asynchronously. Returns 409 if already running.</summary>
        [HttpPost("{scenarioName}/trigger")]
        public async Task<IActionResult> TriggerScenario([FromRoute] string scenarioName, CancellationToken ct)
        {
            _logger.LogInformation("Trigger requested for scenario '{Name}'", scenarioName);
            bool triggered = await _scenarioService.TriggerAsync(scenarioName, ct);
            if (!triggered)
                return Conflict(new { error = $"Scenario '{scenarioName}' not found or already running." });

            return Accepted(new { message = $"Scenario '{scenarioName}' started." });
        }

        /// <summary>Returns all scenario definitions including their steps and recovery steps.</summary>
        [HttpGet("definitions")]
        public IActionResult GetDefinitions()
        {
            var definitions = _scenarioService.GetDefinitions().Select(d => new
            {
                name = d.Name,
                recoveryMs = d.RecoveryMs,
                steps = d.Steps.Select(s => new
                {
                    delayMs = s.DelayMs,
                    ca = s.Ca,
                    oa = s.Oa,
                    valueStr = s.ValueStr,
                    freeze = s.Freeze,
                    description = s.Description
                }),
                recoverySteps = d.RecoverySteps.Select(s => new
                {
                    delayMs = s.DelayMs,
                    ca = s.Ca,
                    oa = s.Oa,
                    valueStr = s.ValueStr,
                    freeze = s.Freeze,
                    description = s.Description
                })
            });
            return Ok(definitions);
        }
    }
}
