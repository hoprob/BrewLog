using Akka.Actor;
using brewlog.api.Actors;
using brewlog.api.Extentions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace brewlog.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoolController : ControllerBase
    {
        private readonly ActorSystem _actorSystem;

        public CoolController(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        [HttpPost]
        [Route("post-cooling-values")]
        public async Task<ActionResult> LogPostCoolingValues(string sessionActorPath, double og, double volumeInFermentationVessle)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(new BrewSessionActor.Commands.ReportPostCoolingValues(og, volumeInFermentationVessle));

            return Ok();
        }
    }
}
