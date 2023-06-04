using Akka.Actor;
using brewlog.application.Actors;
using brewlog.application.Extentions;
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
        public async Task<ActionResult> GetPhLoweringAcid(string sessionActorPath)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();
            try
            {
                var response = await sessionActor.Ask<BrewSessionActor.Responses.GetPhLoweringAcidResponse>(
                    new BrewSessionActor.Queries.GetPhLoweringAcid());

                return Ok(response.MlLacticAcid);
            }
            catch (AskTimeoutException ex)
            {
                return BadRequest(new { Error = "Actor AskTimeoutExeption. Most likely, the command or query is not avalable in the Actor behavior", ExceptionMessage = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            };
        }

        [HttpPost]
        [Route("reportph")]
        public async Task<ActionResult> ReportMashPh(string sessionActorPath, [FromBody] BrewSessionActor.Commands.AddPhValue command)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(command);

            return Ok();
        }
    }
}
