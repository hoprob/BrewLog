using Akka.Actor;
using brewlog.application.Actors;
using brewlog.application.Extentions;
using Microsoft.AspNetCore.Mvc;
using static brewlog.application.Actors.BrewSessionActor.Commands;

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

            try
            {
                var response = await sessionActor.Ask<BrewSessionActor.Responses.YeastViabilityResponse>(
                    new BrewSessionActor.Queries.GetYeastViability());

                return Ok(new { viabillityPercentage = response.ViabilityPercentage, calculatedCellsInPackage = response.CalculatedCellsInPackage });
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
        [Route("yeastpackage")]
        public async Task<IActionResult> EnterYeastPackageValues(string sessionActorPath, [FromBody] EnterValuesForYeastPackage command)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(command);

            return Ok();
        }

        [HttpGet]
        [Route("dme")]
        public async Task<ActionResult<double>> GetGramsOfDME(string sessionActorPath, [FromBody] BrewSessionActor.Queries.GetGramsOfDMENeeded command)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            try
            {
                var response = await sessionActor.Ask<BrewSessionActor.Responses.GetGramsOfDMENeededResponse>(command);

                return Ok(response.GramsOfDME);
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
        [Route("totalcells")]
        public async Task<ActionResult<double>> GetTotalYeastCells(string sessionActorPath)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            try
            {
                var response = await sessionActor.Ask<BrewSessionActor.Responses.GetTotalYeastCellsResponse>(
                    new BrewSessionActor.Queries.GetTotalYeastCells());

                return Ok(response.TotalYeastCells);
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

            try
            {
                var response = await sessionActor.Ask<BrewSessionActor.Responses.YeastCellsNeededResponse>(new BrewSessionActor.Queries.GetYeastCellsNeeded());

                return Ok(response.CellsNeeded);
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
        [Route("yeaststarter")]
        public async Task<ActionResult> AddYeastStarters(string sessionActorPath, [FromBody] BrewSessionActor.Commands.StoreYeastStarters command)
        {
            var sessionActor = _actorSystem.ActorSelection(sessionActorPath);

            if (!await sessionActor.ActorExists())
                return NotFound();

            sessionActor.Tell(command);

            return Ok();
        }

        [HttpGet]
        [Route("producedcells")]
        public async Task<ActionResult> GetProducedCells([FromBody] YeastCalculatorActor.Queries.GetStarterProducedCells query)
        {
            var yeastCalculator = _actorSystem.ActorOf<YeastCalculatorActor>();

            try
            {
                var response = await yeastCalculator.Ask<YeastCalculatorActor.Responses.GetStarterProducedCellsResponse>(query);

                return Ok(response.cellsProduced);
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
