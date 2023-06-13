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

            boil.MapPost("/{sessionName}/preboil-values", (HttpContext ctx, string sessionName, BrewSessionActor.Commands.AddPreBoilValues command) => AddPreBoilValues(ctx, sessionName, command))
                .WithDescription("Stores the reported pre-boil values").WithName(nameof(AddPreBoilValues));

            boil.MapPost("/{sessionName}/added-water", (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.AddAdditionalBoilWater command) =>
            LogAddedWater(ctx, sessionName, command)).WithDescription("Stores added water to boil").WithName(nameof(LogAddedWater));

            boil.MapPost("/{sessionName}/extend-boiltime", (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.AddExtendedBoilTime command) =>
             LogExtendedBoilTime(ctx, sessionName, command)).WithDescription("Stores extending boil minutes").WithName(nameof(LogExtendedBoilTime));

            boil.MapPost("/{sessionName}/boil-complete", (string sessionName) => BoilComplete(sessionName))
                .WithDescription("Completes the voil stage and moves to cooling stage").WithName(nameof(BoilComplete));
        }

        public async Task<IResult> GetSuggestedSgAdjustment(string sessionName)
        {
            var response = await _actorSystem.AskBrewSession<BrewSessionActor.Responses.GetSuggestedBoilSgAdjustmentResponse>(sessionName,
                    new BrewSessionActor.Queries.GetSuggestedBoilSgAdjustment());

            return response.Success ? Results.Ok(response.Response) : Results.BadRequest(response.ErrorMessage);
        }

        public IResult AddPreBoilValues(HttpContext ctx, string sessionName, BrewSessionActor.Commands.AddPreBoilValues command)
        {
            if (command.Validate(ctx) is IEnumerable<ModelError> errors) return Results.UnprocessableEntity(errors);

            _actorSystem.TellBrewSession(sessionName, command);

            return Results.Ok();
        }

        public IResult LogAddedWater(HttpContext ctx, string sessionName, BrewSessionActor.Commands.AddAdditionalBoilWater command)
        {
            if (command.Validate(ctx) is IEnumerable<ModelError> errors) return Results.UnprocessableEntity(errors);

            _actorSystem.TellBrewSession(sessionName, command);

            return Results.Ok();
        }

        public IResult LogExtendedBoilTime(HttpContext ctx, string sessionName, BrewSessionActor.Commands.AddExtendedBoilTime command)
        {
            if (command.Validate(ctx) is IEnumerable<ModelError> errors) return Results.UnprocessableEntity(errors);

            _actorSystem.TellBrewSession(sessionName, command);

            return Results.Ok();
        }

        public IResult BoilComplete(string sessionName)
        {
            _actorSystem.TellBrewSession(sessionName, new BrewSessionActor.Commands.BoilStageComplete());

            return Results.Ok();
        }
    }
}
