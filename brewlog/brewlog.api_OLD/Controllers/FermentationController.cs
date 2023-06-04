using Akka.Actor;
using brewlog.application.Actors;
using brewlog.application.Extentions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;

namespace brewlog.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FermentationController : ControllerBase
    {
        private readonly ActorSystem _actorSystem;

        public FermentationController(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        [HttpPost]
        [Route("fermentation-value")]
        public async Task<ActionResult> AddFermentationValue(string sessionActorPath, [FromBody] BrewSessionActor.Commands.AddFermentationValue command)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(command);

            return Ok();
        }

        [HttpPost]
        [Route("change-temperature")]
        public async Task<ActionResult> ChangeFermentationTemperature(string sessionActorPath, [FromBody] BrewSessionActor.Commands.ChangeFermentationTemperature command)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(command);

            return Ok();
        }

        [HttpGet]
        [Route("current-abv")]
        public async Task<ActionResult> GetCurrentAbv(string sessionActorPath)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();
            try
            {
                var response = await sessionActor.Ask<BrewSessionActor.Responses.GetFermentationAbvResponse>(
                    new BrewSessionActor.Queries.GetFermentationAbv());

                return Ok(response);
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
        [Route("fermentation-complete")]
        public async Task<ActionResult> FermentationStageComplete(string sessionActorPath, [FromBody] BrewSessionActor.Commands.FermentationStageComplete command)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(command);

            return Ok();
        }
    }
}
