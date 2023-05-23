using Akka.Actor;
using brewlog.api.Actors;
using brewlog.api.Extentions;
using Microsoft.AspNetCore.Http;
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
            var response = await sessionActor.Ask<BrewSessionActor.Responses.GetBrewSessionStateResponse>(
                new BrewSessionActor.Queries.GetBrewSessionState());

            return Ok(response.State);

        }

        [HttpPost]
        [Route("new")]
        public IActionResult CreateNewSession(string sessionName)
        {
            IActorRef sessionActor = _actorSystem.ActorOf<BrewSessionActor>($"{sessionName}");

            return new ObjectResult(sessionActor.Path.ToString()) { StatusCode = StatusCodes.Status201Created };
        }

        [HttpPost]
        [Route("reciepe-input-complete")] //TODO Gets stuck.....
        public async Task<ActionResult> RecipeInputComplete(string sessionActorPath)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            var response = await sessionActor.Ask<BrewSessionActor.Responses.RecipeInputCompleteResponse>(
                new BrewSessionActor.Commands.ReciepeInputComplete());
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                return BadRequest(response.ErrorMessage);
            }             
            return Ok();
        }
    }
}
