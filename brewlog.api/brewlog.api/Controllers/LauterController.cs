using Akka.Actor;
using brewlog.api.Actors;
using brewlog.api.Extentions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace brewlog.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LauterController : ControllerBase
    {
        private readonly ActorSystem _actorSystem;

        public LauterController(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        [HttpPost]
        [Route("waterinlauter")]
        public async Task<ActionResult> TotalWaterInLauter(string sessionActorPath, double litresWater)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(new BrewSessionActor.Commands.AddTotalWaterInLauter(litresWater));

            return Ok();
        }
    }
}
