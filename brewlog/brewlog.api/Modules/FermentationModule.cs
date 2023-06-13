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

            fermentation.MapPost("/{sessionName}/fermentation-value", (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.AddFermentationValue command) =>
            AddFermentationValue(ctx, sessionName, command)).WithDescription("Stores specified fermentation values").WithName(nameof(AddFermentationValue));

            fermentation.MapPost("/{sessionName}/set-temperature", (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.ChangeFermentationTemperature command) =>
            SetFermentationTemperature(ctx, sessionName, command)).WithDescription("Sets the desired fermentation temperature").WithName(nameof(SetFermentationTemperature));

            fermentation.MapPost("/{sessionName}/fermentation-complete", (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.FermentationStageComplete command) =>
            FermentationStageComplete(ctx, sessionName, command)).WithDescription("Completes the fermentation stage and moves to bottling stage").WithName(nameof(FermentationStageComplete));
        }

        private async Task<IResult> GetCurrentAbv(string sessionName)
        {
            var response = await _actorSystem.AskBrewSession<BrewSessionActor.Responses.GetFermentationAbvResponse>(sessionName,
                    new BrewSessionActor.Queries.GetFermentationAbv());

            return response.Success ? Results.Ok(response.Response) : Results.BadRequest(response.ErrorMessage); 
        }

        private IResult AddFermentationValue(HttpContext ctx, string sessionName, BrewSessionActor.Commands.AddFermentationValue command)
        {
            if (command.Validate(ctx) is IEnumerable<ModelError> errors) return Results.UnprocessableEntity(errors);

            _actorSystem.TellBrewSession(sessionName, command);

            return Results.Ok();
        }

        private IResult SetFermentationTemperature(HttpContext ctx, string sessionName,  BrewSessionActor.Commands.ChangeFermentationTemperature command)
        {
            if (command.Validate(ctx) is IEnumerable<ModelError> errors) return Results.UnprocessableEntity(errors);

            _actorSystem.TellBrewSession(sessionName, command);

            return Results.Ok();
        }

        private IResult FermentationStageComplete(HttpContext ctx, string sessionName,  BrewSessionActor.Commands.FermentationStageComplete command)
        {
            if (command.Validate(ctx) is IEnumerable<ModelError> errors) return Results.UnprocessableEntity(errors);

            _actorSystem.TellBrewSession(sessionName, command);

            return Results.Ok();
        }
    }
}
