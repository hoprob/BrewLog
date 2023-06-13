using Akka.Actor;
using brewlog.api.Extentions;
using brewlog.application.Actors;
using brewlog.application.Extentions;
using Carter;
using Carter.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using static brewlog.application.Actors.BrewSessionActor.Commands;

namespace brewlog.api
{
    public class YeastStarterModule : ICarterModule
    {
        private readonly ActorSystem _actorSystem;
        public YeastStarterModule(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var yeastStarter = app.MapGroup("/yeast-starter").WithTags("Yeast starter").WithOpenApi();

            yeastStarter.MapGet("viability/{sessionName}", async (string sessionName) => await GetYeastViability(sessionName)).WithDescription("Gets viability % of registred yeast package").WithName(nameof(GetYeastViability));

            yeastStarter.MapGet("dme/{sessionName}", async (HttpContext ctx, string sessionName,[AsParameters] BrewSessionActor.Queries.GetGramsOfDMENeeded command) => await GetGramsOfDme(ctx, sessionName, command))
                .WithDescription("Gets grams of DME to reach 1.037 based on litres of water").WithName(nameof(GetGramsOfDme));

            yeastStarter.MapGet("total-cells/{sessionName}", async (string sessionName) => await GetTotalYeastCells(sessionName)).WithDescription("Gets the total yeast cells including registered yeast-starters and yeast-package").WithName(nameof(GetTotalYeastCells));

            yeastStarter.MapGet("cells-needed/{sessionName}", async (string sessionName) => await GetYeastCellsNeeded(sessionName)).WithDescription("Gets the amount of yeastcells needed for the OG").WithName(nameof(GetYeastCellsNeeded));

            yeastStarter.MapGet("produced-cells", async (HttpContext ctx, [AsParameters] YeastCalculatorActor.Queries.GetStarterProducedCells query) => await GetProducedCells(ctx, query))
                .WithDescription("Gets produced yeastcells based on grams of DME and initial yeastcells").WithName(nameof(GetProducedCells));

            yeastStarter.MapPost("yeast-package/{sessionName}", (HttpContext ctx, string sessionName, [FromBody] EnterValuesForYeastPackage command) => EnterYeastPackageValues(ctx, sessionName, command))
                .WithDescription("Stores the specified yeast package values").WithName(nameof(EnterYeastPackageValues));

            yeastStarter.MapPost("yeast-starter-complete/{sessionName}", (string sessionName) => YeastStarterComplete(sessionName)).WithDescription("Complete the yeast starter stage and moves to mash stage").WithName(nameof(YeastStarterComplete));

            yeastStarter.MapPost("yeast-starters/{sessionName}", (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.StoreYeastStarters command) => AddYeastStarters(ctx, sessionName, command))
                .WithDescription("Stores the specified starters").WithName(nameof(AddYeastStarters));
        }

        private async Task<IResult> GetYeastViability(string sessionName)
        {
            var response = await _actorSystem.AskBrewSession<BrewSessionActor.Responses.YeastViabilityResponse>(
                    sessionName, new BrewSessionActor.Queries.GetYeastViability());

            return response.Success ? Results.Ok(new { viabillityPercentage = response.Response.ViabilityPercentage, calculatedCellsInPackage = response.Response.CalculatedCellsInPackage }) :
                Results.BadRequest(response.ErrorMessage);
        }

        private async Task<IResult> GetGramsOfDme(HttpContext ctx, string sessionName, BrewSessionActor.Queries.GetGramsOfDMENeeded query)
        {
            if (query.Validate(ctx) is IEnumerable<ModelError> errors) return Results.UnprocessableEntity(errors);

            var response = await _actorSystem.AskBrewSession<BrewSessionActor.Responses.GetGramsOfDMENeededResponse>(sessionName, query);

            return response.Success ? Results.Ok(response.Response.GramsOfDME) : Results.BadRequest(response.ErrorMessage);
        }

        private async Task<IResult> GetTotalYeastCells(string sessionName)
        {
            var response = await _actorSystem.AskBrewSession<BrewSessionActor.Responses.GetTotalYeastCellsResponse>(
                    sessionName, new BrewSessionActor.Queries.GetTotalYeastCells());

            return response.Success ? Results.Ok(response.Response.TotalYeastCells) : Results.BadRequest(response.ErrorMessage);
        }

        private async Task<IResult> GetYeastCellsNeeded(string sessionName)
        {
            var response = await _actorSystem.AskBrewSession<BrewSessionActor.Responses.YeastCellsNeededResponse>(
                sessionName, new BrewSessionActor.Queries.GetYeastCellsNeeded());

            return response.Success ? Results.Ok(response.Response.CellsNeeded) : Results.BadRequest(response.ErrorMessage);
        }

        private async Task<IResult> GetProducedCells(HttpContext ctx, YeastCalculatorActor.Queries.GetStarterProducedCells query) //TODO Go through brewsessionactor?
        {
            if (query.Validate(ctx) is IEnumerable<ModelError> errors) return Results.UnprocessableEntity(errors);

            var yeastCalculator = _actorSystem.ActorOf<YeastCalculatorActor>();

            try
            {
                var response = await yeastCalculator.Ask<YeastCalculatorActor.Responses.GetStarterProducedCellsResponse>(query);

                return Results.Ok(response.cellsProduced);
            }
            catch (Exception ex)
            {
                return ex.ApiActorResponse();
            };
        }

        private IResult EnterYeastPackageValues(HttpContext ctx, string sessionName, EnterValuesForYeastPackage command)
        {
            if (command.Validate(ctx) is IEnumerable<ModelError> errors) return Results.UnprocessableEntity(errors);

            _actorSystem.TellBrewSession(sessionName, command);

            return Results.Ok();
        }

        private IResult YeastStarterComplete(string sessionName)
        {
            _actorSystem.TellBrewSession(sessionName, new BrewSessionActor.Commands.YeastStarterComplete());
   
            return Results.Ok();
        }

        private IResult AddYeastStarters(HttpContext ctx, string sessionName, BrewSessionActor.Commands.StoreYeastStarters command)
        {
            if (command.Validate(ctx) is IEnumerable<ModelError> errors) return Results.UnprocessableEntity(errors);

            _actorSystem.TellBrewSession(sessionName, command);

            return Results.Ok();
        }
    }
}
