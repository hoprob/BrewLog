using Akka.Actor;
using brewlog.api.Actors;
using brewlog.api.Extentions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace brewlog.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MashController : ControllerBase
    {
        private readonly ActorSystem _actorSystem;

        public MashController(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        [HttpGet]
        [Route("lacticacid")]
        public async Task<ActionResult> GetPhLoweringAcid(string sessionActorPath) //TODO Gets stuck somewhere... debug!
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            var response = await sessionActor.Ask<BrewSessionActor.Responses.GetPhLoweringAcidResponse>(
                new BrewSessionActor.Queries.GetPhLoweringAcid());

            return Ok(response.MlLacticAcid);
        }

        [HttpPost]
        [Route("reportph")]
        public async Task<ActionResult> ReportMashPh(string sessionActorPath, double mashPh)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(new BrewSessionActor.Commands.AddPhValue(mashPh));

            return Ok();
        }
    }
}
