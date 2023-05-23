using Akka.Actor;
using brewlog.api.Actors;
using brewlog.api.Extentions;
using brewlog.api.Models;
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
        public async Task<ActionResult> AddFermentationValue(string sessionActorPath, FermentationValue fermentationValue) //TODO DTO
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(new BrewSessionActor.Commands.AddFermentationValue(fermentationValue));

            return Ok();
        }

        [HttpPost]
        [Route("change-temperature")]
        public async Task<ActionResult> ChangeFermentationTemperature(string sessionActorPath, double temperature)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(new BrewSessionActor.Commands.ChangeFermentationTemperature(temperature));

            return Ok();
        }

        [HttpGet]
        [Route("current-abv")]
        public async Task<ActionResult> GetCurrentAbv(string sessionActorPath) //TODO Get all values based on fermentationvalues?
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            var response = await sessionActor.Ask<BrewSessionActor.Responses.GetFermentationAbvResponse>(
                new BrewSessionActor.Queries.GetFermentationAbv());

            return Ok(response);
        }

        [HttpPost]
        [Route("fermentation-complete")]
        public async Task<ActionResult> FermentationStageComplete(string sessionActorPath, double fg)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(new BrewSessionActor.Commands.FermentationStageComplete(fg));

            return Ok();
        }
    }
}
