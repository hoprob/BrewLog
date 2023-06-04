using Akka.Actor;
using brewlog.application.Actors;
using brewlog.application.Extentions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static brewlog.application.Actors.BrewSessionActor.Queries;

namespace brewlog.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoilController : ControllerBase
    {
        private readonly ActorSystem _actorSystem;

        public BoilController(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        [HttpGet]
        [Route("sgadjustment")]
        public  async Task<ActionResult> GetSuggestedSgAdjustment(string sessionActorPath)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();
           
            try
            {
                var result = await sessionActor.Ask<BrewSessionActor.Responses.GetSuggestedBoilSgAdjustmentResponse>(
                    new BrewSessionActor.Queries.GetSuggestedBoilSgAdjustment());

                return Ok(result);
            }
            catch (AskTimeoutException ex)
            {
                return BadRequest(new { Error = "Actor AskTimeoutExeption. Most likely, the command or query is not avalable in the Actor behavior", ExceptionMessage = ex.Message });
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            };         
        }

        [HttpPost]
        [Route("preboilsg")]
        public async Task<ActionResult> EnterPreBoilSg(string sessionActorPath, [FromBody] BrewSessionActor.Commands.AddPreBoilSg command)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(command);

            return Ok();
        }

        [HttpPost]
        [Route("boilvolume")]
        public async Task<ActionResult> EnterBoilVolume(string sessionActorPath, [FromBody] BrewSessionActor.Commands.AddBoilVolume command)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(command);

            return Ok();
        }

        [HttpPost]
        [Route("added-water")]
        public async Task<ActionResult> LogAddedWater(string sessionActorPath, [FromBody] BrewSessionActor.Commands.AddAdditionalBoilWater command)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(command);

            return Ok();
        }

        [HttpPost]
        [Route("extend-boiltime")]
        public async Task<ActionResult> LogExtendedBoilTime(string sessionActorPath, [FromBody] BrewSessionActor.Commands.AddExtendedBoilTime command)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(command);

            return Ok();
        }

        [HttpPost]
        [Route("boil-complete")]
        public async Task<ActionResult> BoilComplete(string sessionActorPath)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(new BrewSessionActor.Commands.BoilStageComplete());

            return Ok();
        }
    }
}
