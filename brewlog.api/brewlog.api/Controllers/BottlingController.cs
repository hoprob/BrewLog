using Akka.Actor;
using brewlog.api.Actors;
using brewlog.api.Extentions;
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
        public async Task<ActionResult> ChangeStorageTemperature(string sessionActorPath, double temperature)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(new BrewSessionActor.Commands.ChangeBottlingStorageTemperature(temperature));

            return Ok();
        }

        [HttpPost]
        [Route("desired-co2-volume")]
        public async Task<ActionResult> SetDesiredCO2Colume(string sessionActorPath, double co2Volume)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(new BrewSessionActor.Commands.SetDesiredCo2Colume(co2Volume));

            return Ok();
        }

        [HttpGet]
        [Route("co2-pressure-psi")]
        public async Task<ActionResult> GetNeededCo2PressureInPsi(string sessionActorPath)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            var response = await sessionActor.Ask<BrewSessionActor.Responses.GetCarbonationPressureInPsiRespone>
                (new BrewSessionActor.Queries.GetCarbonationPressureInPsi());

            return Ok(response.psiPressure);
        }
    }
}
