using Akka.Actor;
using brewlog.application.Actors;
using brewlog.application.Extentions;
using Microsoft.AspNetCore.Mvc;

namespace brewlog.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly ActorSystem _actorSystem;
        public SessionController(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        [HttpGet]
        [Route("state")]
        public async Task<ActionResult> GetSessionState(string sessionActorPath)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            try
            {
                var response = await sessionActor.Ask<BrewSessionActor.Responses.GetBrewSessionStateResponse>(
                    new BrewSessionActor.Queries.GetBrewSessionState());

                return Ok(response.State);
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

        [HttpGet]
        [Route("values")]
        public async Task<ActionResult> GetSessionValues(string sessionActorPath)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            try
            {
                var response = await sessionActor.Ask<BrewSessionActor.Responses.BrewSessionValuesResponse>(
                    new BrewSessionActor.Queries.GetBrewSessionValues());

                return Ok(response.SessionValues);
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
        [Route("new")]
        public IActionResult CreateNewSession(string sessionName)
        {
            IActorRef sessionActor = _actorSystem.ActorOf<BrewSessionActor>($"{sessionName}");

            return new ObjectResult(sessionActor.Path.ToString()) { StatusCode = StatusCodes.Status201Created };
        }

        [HttpPost]
        [Route("recipe")]
        public async Task<ActionResult> AddSessionRecipe(string actorPath, [FromBody] BrewSessionActor.Commands.AddSessionRecipe command)
        {

            var sessionActor = _actorSystem.ActorSelection(actorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            try
            {
                var response = sessionActor.Ask<BrewSessionActor.Responses.AddSessionRecipeResponse>(command);
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
    }
}
