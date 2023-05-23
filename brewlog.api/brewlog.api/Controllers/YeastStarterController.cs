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
    public class YeastStarterController : ControllerBase
    {
        private readonly ActorSystem _actorSystem;

        public YeastStarterController(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }
        [HttpGet]
        [Route("viability")]
        public async Task<IActionResult> GetYeastViability(string sessionActorPath)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            var response = await sessionActor.Ask<BrewSessionActor.Responses.YeastViabilityResponse>(
                new BrewSessionActor.Queries.GetYeastViability());
            return Ok(new { viabillityPercentage = response.ViabilityPercentage, calculatedCellsInPackage = response.CalculatedCellsInPackage });
        }
        [HttpPost]
        [Route("yeastpackage")]
        public async Task<IActionResult> EnterYeastPackageValues(string sessionActorPath, [FromQuery] DateTimeOffset productionDate, [FromQuery] double initialYeastCells)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(new BrewSessionActor.Commands.EnterValuesForYeastPackage(productionDate, initialYeastCells));

            return Ok();
        }
        [HttpGet]
        [Route("dme")]
        public async Task<ActionResult<double>> GetGramsOfDME(string sessionActorPath, [FromQuery] double litresWater)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            var response = await sessionActor.Ask<BrewSessionActor.Responses.GetGramsOfDMENeededResponse>(
                new BrewSessionActor.Queries.GetGramsOfDMENeeded(litresWater));

            return Ok(response.GramsOfDME);
        }
        [HttpGet]
        [Route("totalcells")]
        public async Task<ActionResult<double>> GetTotalYeastCells(string sessionActorPath)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            var response = await sessionActor.Ask<BrewSessionActor.Responses.GetTotalYeastCellsResponse>(
                new BrewSessionActor.Queries.GetTotalYeastCells());

            return Ok(response.TotalYeastCells);
        }
        [HttpPost]
        [Route("yeaststartercomplete")]
        public async Task<ActionResult> YeastStarterStateComplete(string sessionActorPath)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(new BrewSessionActor.Commands.YeastStarterComplete());

            return Ok();
        }

        [HttpGet]
        [Route("cellsneeded")]
        public async Task<ActionResult> GetYeastCellsNeeded(string sessionActorPath)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            var response = await sessionActor.Ask<BrewSessionActor.Responses.YeastCellsNeededResponse>(new BrewSessionActor.Queries.GetYeastCellsNeeded());

            return Ok(response.CellsNeeded);
        }
        [HttpPost]
        [Route("yeaststarter")]
        public async Task<ActionResult> AddYeastStarters(string sessionActorPath, List<YeastStarter> yeastStarters)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(new BrewSessionActor.Commands.StoreYeastStarters(yeastStarters));

            return Ok();
        }

        [HttpGet]
        [Route("producedcells")]
        public async Task<ActionResult> GetProducedCells(double gramsOfDme, double initialCells)
        {
            var yeastCalculator = _actorSystem.ActorOf<YeastCalculatorActor>();

            var response = await yeastCalculator.Ask<YeastCalculatorActor.Responses.GetStarterProducedCellsResponse>(
                new YeastCalculatorActor.Queries.GetStarterProducedCells(gramsOfDme, initialCells));

            return Ok(response.cellsProduced);
        }

    }
}
