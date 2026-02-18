using IEC60870_5_104_simulator.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IEC60870_5_104_simulator.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly IProfileProvider _profileProvider;

        public ProfilesController(IProfileProvider profileProvider)
        {
            _profileProvider = profileProvider;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return _profileProvider.GetProfileNames();
        }
    }
}
