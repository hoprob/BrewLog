using Akka.Actor;
using Carter;
using brewlog.application.Extentions;
using brewlog.application.Actors;
using Microsoft.AspNetCore.Mvc;
using Carter.ModelBinding;
using brewlog.api.Extentions;

namespace brewlog.api.Modules
{
    public class BoilModule : ICarterModule
    {
        private readonly ActorSystem _actorSystem;
        public BoilModule(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var boil = app.MapGroup("boil").WithTags("Boil").WithOpenApi();

            boil.MapGet("/{sessionName}/sg-adjustment", async (string sessionName) => await GetSuggestedSgAdjustment(sessionName))
                .WithDescription("Gets suggestion (extend boil time or dilute with water) to reach target og based on pre-boil volume and pre-boil sg.").WithName(nameof(GetSuggestedSgAdjustment));

            boil.MapPost("/{sessionName}/preboil-values", async (HttpContext ctx, string sessionName, BrewSessionActor.Commands.AddPreBoilValues command) => await AddPreBoilValues(ctx, sessionName, command))
                .WithDescription("Stores the reported pre-boil values").WithName(nameof(AddPreBoilValues));

            boil.MapPost("/{sessionName}/added-water", async (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.AddAdditionalBoilWater command) =>
            await LogAddedWater(ctx, sessionName, command)).WithDescription("Stores added water to boil").WithName(nameof(LogAddedWater));

            boil.MapPost("/{sessionName}/extend-boiltime", async (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.AddExtendedBoilTime command) =>
            await LogExtendedBoilTime(ctx, sessionName, command)).WithDescription("Stores extending boil minutes").WithName(nameof(LogExtendedBoilTime));

            boil.MapPost("/{sessionName}/boil-complete", async (string sessionName) => await BoilComplete(sessionName))
                .WithDescription("Completes the voil stage and moves to cooling stage").WithName(nameof(BoilComplete));
        }

        public async Task<IResult> GetSuggestedSgAdjustment(string sessionName)
        {
            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            try
            {
                var result = await sessionActor.Ask<BrewSessionActor.Responses.GetSuggestedBoilSgAdjustmentResponse>(
                    new BrewSessionActor.Queries.GetSuggestedBoilSgAdjustment());

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return ex.ApiActorResponse();
            };
        }

        public async Task<IResult> AddPreBoilValues(HttpContext ctx, string sessionName, BrewSessionActor.Commands.AddPreBoilValues command)
        {
            var validation = ctx.Request.Validate(command);
            if (!validation.IsValid) return Results.UnprocessableEntity(validation.GetFormattedErrors());

            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            sessionActor.Tell(command);

            return Results.Ok();
        }

        public async Task<IResult> LogAddedWater(HttpContext ctx, string sessionName, BrewSessionActor.Commands.AddAdditionalBoilWater command)
        {
            var validation = ctx.Request.Validate(command);
            if (!validation.IsValid) return Results.UnprocessableEntity(validation.GetFormattedErrors());

            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            sessionActor.Tell(command);

            return Results.Ok();
        }

        public async Task<IResult> LogExtendedBoilTime(HttpContext ctx, string sessionName, BrewSessionActor.Commands.AddExtendedBoilTime command)
        {
            var validation = ctx.Request.Validate(command);
            if (!validation.IsValid) return Results.UnprocessableEntity(validation.GetFormattedErrors());

            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            sessionActor.Tell(command);

            return Results.Ok();
        }

        public async Task<IResult> BoilComplete(string sessionName)
        {
            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            sessionActor.Tell(new BrewSessionActor.Commands.BoilStageComplete());

            return Results.Ok();
        }
    }
}
