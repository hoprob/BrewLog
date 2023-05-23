using Akka.Actor;
using brewlog.api.Actors;
using brewlog.api.Extentions;
using brewlog.api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace brewlog.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ActorSystem _actorSystem;
        public ValuesController(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        [HttpGet]
        public async Task<ActionResult> GetSessionValues(string actorPath)
        {
            var sessionActor = _actorSystem.ActorSelection(actorPath);
            if (!await sessionActor.ActorExists())
                return NotFound();
            var response = await sessionActor.Ask<BrewSessionActor.Responses.BrewSessionValuesResponse>(
                new BrewSessionActor.Queries.GetBrewSessionValues());
            return Ok(response.SessionValues);
        }

        [HttpPost]
        public async Task<ActionResult> SetSessionValues(string actorPath, SessionValues sessionValues) //TODO DTO
        {
            var sessionActor = _actorSystem.ActorSelection(actorPath);
            if (!await sessionActor.ActorExists())
                return NotFound();
            var response = sessionActor.Ask<BrewSessionActor.Responses.AddBrewSessionValuesResponse>(new 
                BrewSessionActor.Commands.AddBrewSessionValues(sessionValues));
            return Ok(response);
        }
    }
}
