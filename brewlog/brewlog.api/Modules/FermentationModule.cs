using Akka.Actor;
using brewlog.api.Extentions;
using brewlog.application.Actors;
using brewlog.application.Extentions;
using Carter;
using Carter.ModelBinding;
using Microsoft.AspNetCore.Mvc;

namespace brewlog.api.Modules
{
    public class FermentationModule : ICarterModule
    {
        private readonly ActorSystem _actorSystem;
        public FermentationModule(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var fermentation = app.MapGroup("/fermentation").WithTags("Fermentation").WithOpenApi();

            fermentation.MapGet("/{sessionName}/current-abv", async (string sessionName) => await GetCurrentAbv(sessionName))
                .WithDescription("Gets the current abv based on OG and last reported SG").WithName(nameof(GetCurrentAbv));

            fermentation.MapPost("/{sessionName}/fermentation-value", async (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.AddFermentationValue command) =>
            await AddFermentationValue(ctx, sessionName, command)).WithDescription("Stores specified fermentation values").WithName(nameof(AddFermentationValue));

            fermentation.MapPost("/{sessionName}/set-temperature", async (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.ChangeFermentationTemperature command) =>
            await SetFermentationTemperature(ctx, sessionName, command)).WithDescription("Sets the desired fermentation temperature").WithName(nameof(SetFermentationTemperature));

            fermentation.MapPost("/{sessionName}/fermentation-complete", async (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.FermentationStageComplete command) =>
            await FermentationStageComplete(ctx, sessionName, command)).WithDescription("Completes the fermentation stage and moves to bottling stage").WithName(nameof(FermentationStageComplete));
        }

        private async Task<IResult> GetCurrentAbv(string sessionName)
        {
            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            try
            {
                var response = await sessionActor.Ask<BrewSessionActor.Responses.GetFermentationAbvResponse>( //TODO Extention
                    new BrewSessionActor.Queries.GetFermentationAbv());

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return ex.ApiActorResponse();
            };
        }

        private async Task<IResult> AddFermentationValue(HttpContext ctx, string sessionName, BrewSessionActor.Commands.AddFermentationValue command)
        {
            var validation = ctx.Request.Validate(command);
            if (!validation.IsValid) return Results.UnprocessableEntity(validation.GetFormattedErrors());

            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            sessionActor.Tell(command);

            return Results.Ok();
        }

        private async Task<IResult> SetFermentationTemperature(HttpContext ctx, string sessionName,  BrewSessionActor.Commands.ChangeFermentationTemperature command)
        {
            var validation = ctx.Request.Validate(command);
            if (!validation.IsValid) return Results.UnprocessableEntity(validation.GetFormattedErrors());

            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            sessionActor.Tell(command);

            return Results.Ok();
        }

        private async Task<IResult> FermentationStageComplete(HttpContext ctx, string sessionName,  BrewSessionActor.Commands.FermentationStageComplete command)
        {
            var validation = ctx.Request.Validate(command);
            if (!validation.IsValid) return Results.UnprocessableEntity(validation.GetFormattedErrors());

            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            sessionActor.Tell(command);

            return Results.Ok();
        }
    }
}
