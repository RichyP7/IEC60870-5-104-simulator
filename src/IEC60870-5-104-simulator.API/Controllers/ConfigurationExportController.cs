using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using IEC60870_5_104_simulator.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace IEC60870_5_104_simulator.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationExportController : ControllerBase
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() },
        };

        private readonly ConfigurationExportService _exportService;

        public ConfigurationExportController(ConfigurationExportService exportService)
        {
            _exportService = exportService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var document = _exportService.BuildExportDocument();
            string json = JsonSerializer.Serialize(document, SerializerOptions);
            return File(Encoding.UTF8.GetBytes(json), "application/json", "SimulationOptions.json");
        }
    }
}
