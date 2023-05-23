using Akka.Actor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace brewlog.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhController : ControllerBase
    {
        private readonly ActorSystem _actorSystem;
        public PhController(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        [HttpGet]
        public IActionResult GetTest()
        {
            var systemName = _actorSystem.Name;
            return Ok(systemName);
        }
    }
}
