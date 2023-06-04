using Akka.Actor;
using brewlog.application.Actors;
using brewlog.application.Extentions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace brewlog.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BottlingController : ControllerBase
    {
        private readonly ActorSystem _actorSystem;
        public BottlingController(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        [HttpPost]
        [Route("storage-temperature")]
        public async Task<ActionResult> ChangeStorageTemperature(string sessionActorPath, [FromBody] BrewSessionActor.Commands.ChangeBottlingStorageTemperature command)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(command);

            return Ok();
        }

        [HttpPost]
        [Route("desired-co2-volume")]
        public async Task<ActionResult> SetDesiredCO2Colume(string sessionActorPath, [FromBody] BrewSessionActor.Commands.SetDesiredCo2Colume command)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(command);

            return Ok();
        }

        [HttpGet]
        [Route("co2-pressure-psi")]
        public async Task<ActionResult> GetNeededCo2PressureInPsi(string sessionActorPath)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            try
            {
                var response = await sessionActor.Ask<BrewSessionActor.Responses.GetCarbonationPressureInPsiRespone>
                (new BrewSessionActor.Queries.GetCarbonationPressureInPsi());

                return Ok(response.psiPressure);
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
    }
}
