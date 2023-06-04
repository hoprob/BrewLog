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

            yeastStarter.MapGet("viability/{sessionName}", (string sessionName) => GetYeastViability(sessionName)).WithDescription("Gets viability % of registred yeast package").WithName(nameof(GetYeastViability));

            yeastStarter.MapGet("dme/{sessionName}", async (HttpContext ctx, string sessionName,[AsParameters] BrewSessionActor.Queries.GetGramsOfDMENeeded command) => await GetGramsOfDme(ctx, sessionName, command))
                .WithDescription("Gets grams of DME to reach 1.037 based on litres of water").WithName(nameof(GetGramsOfDme));

            yeastStarter.MapGet("total-cells/{sessionName}", (string sessionName) => GetTotalYeastCells(sessionName)).WithDescription("Gets the total yeast cells including registered yeast-starters and yeast-package").WithName(nameof(GetTotalYeastCells));

            yeastStarter.MapGet("cells-needed/{sessionName}", (string sessionName) => GetYeastCellsNeeded(sessionName)).WithDescription("Gets the amount of yeastcells needed for the OG").WithName(nameof(GetYeastCellsNeeded));

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
            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            try
            {
                var response = await sessionActor.Ask<BrewSessionActor.Responses.YeastViabilityResponse>(
                    new BrewSessionActor.Queries.GetYeastViability());

                return Results.Ok(new { viabillityPercentage = response.ViabilityPercentage, calculatedCellsInPackage = response.CalculatedCellsInPackage });
            }
            catch (Exception ex)
            {
                return ex.ApiActorResponse();
            };
        }

        private async Task<IResult> GetGramsOfDme(HttpContext ctx, string sessionName, BrewSessionActor.Queries.GetGramsOfDMENeeded command)
        {
            var validation = ctx.Request.Validate<BrewSessionActor.Queries.GetGramsOfDMENeeded>(command);
            if (!validation.IsValid) return Results.UnprocessableEntity(validation.GetFormattedErrors());

            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            try
            {
                var response = await sessionActor.Ask<BrewSessionActor.Responses.GetGramsOfDMENeededResponse>(command);

                return Results.Ok(response.GramsOfDME);
            }
            catch (Exception ex)
            {
                return ex.ApiActorResponse();
            };
        }

        private async Task<IResult> GetTotalYeastCells(string sessionName)
        {
            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            try
            {
                var response = await sessionActor.Ask<BrewSessionActor.Responses.GetTotalYeastCellsResponse>(
                    new BrewSessionActor.Queries.GetTotalYeastCells());

                return Results.Ok(response.TotalYeastCells);
            }
            catch (Exception ex)
            {
                return ex.ApiActorResponse();
            };
        }

        private async Task<IResult> GetYeastCellsNeeded(string sessionName)
        {
            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            try
            {
                var response = await sessionActor.Ask<BrewSessionActor.Responses.YeastCellsNeededResponse>(new BrewSessionActor.Queries.GetYeastCellsNeeded());

                return Results.Ok(response.CellsNeeded);
            }
            catch (Exception ex)
            {
                return ex.ApiActorResponse();
            };
        }

        private async Task<IResult> GetProducedCells(HttpContext ctx, YeastCalculatorActor.Queries.GetStarterProducedCells query)
        {
            var validation = ctx.Request.Validate<YeastCalculatorActor.Queries.GetStarterProducedCells>(query);
            if (!validation.IsValid) return Results.UnprocessableEntity(validation.GetFormattedErrors());

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

        private async Task<IResult> EnterYeastPackageValues(HttpContext ctx, string sessionName, EnterValuesForYeastPackage command)
        {
            var validation = ctx.Request.Validate<BrewSessionActor.Commands.EnterValuesForYeastPackage>(command);
            if (!validation.IsValid) return Results.UnprocessableEntity(validation.GetFormattedErrors());

            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            sessionActor.Tell(command);

            return Results.Ok();
        }

        private async Task<IResult> YeastStarterComplete(string sessionName)
        {
            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            sessionActor.Tell(new BrewSessionActor.Commands.YeastStarterComplete());

            return Results.Ok();
        }

        private async Task<IResult> AddYeastStarters(HttpContext ctx, string sessionName, BrewSessionActor.Commands.StoreYeastStarters command)
        {
            var validation = ctx.Request.Validate<BrewSessionActor.Commands.StoreYeastStarters>(command);
            if (!validation.IsValid) return Results.UnprocessableEntity(validation.GetFormattedErrors());

            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            sessionActor.Tell(command);

            return Results.Ok();
        }
    }
}
