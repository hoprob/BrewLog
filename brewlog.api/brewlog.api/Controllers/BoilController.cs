using Akka.Actor;
using brewlog.api.Actors;
using brewlog.api.Extentions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static brewlog.api.Actors.BrewSessionActor.Queries;

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

            var result = await sessionActor.Ask<BrewSessionActor.Responses.GetSuggestedBoilSgAdjustmentResponse>(
                new BrewSessionActor.Queries.GetSuggestedBoilSgAdjustment());

            return Ok(result);
        }

        [HttpPost]
        [Route("preboilsg")]
        public async Task<ActionResult> EnterPreBoilSg(string sessionActorPath, double preBoilSg)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(new BrewSessionActor.Commands.AddPreBoilSg(preBoilSg));

            return Ok();
        }

        [HttpPost]
        [Route("boilvolume")]
        public async Task<ActionResult> EnterBoilVolume(string sessionActorPath, double boilVolume)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(new BrewSessionActor.Commands.AddBoilVolume(boilVolume));

            return Ok();
        }

        [HttpPost]
        [Route("added-water")]
        public async Task<ActionResult> LogAddedWater(string sessionActorPath, double waterAdded)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(new BrewSessionActor.Commands.AddAdditionalBoilWater(waterAdded));

            return Ok();
        }

        [HttpPost]
        [Route("extend-boiltime")]
        public async Task<ActionResult> LogExtendedBoilTime(string sessionActorPath, double extendedMinutes)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(new BrewSessionActor.Commands.AddExtendedBoilTime(extendedMinutes));

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


        //TODO PH Adjustment?
    }
}
