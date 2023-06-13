using Akka.Actor;
using brewlog.api.Extentions;
using brewlog.application.Actors;
using brewlog.application.Extentions;
using Carter;
using Carter.ModelBinding;
using Microsoft.AspNetCore.Mvc;

namespace brewlog.api.Modules
{
    public class CoolingModule : ICarterModule
    {
        private readonly ActorSystem _actorSystem;
        public CoolingModule(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var cooling = app.MapGroup("/cooling").WithTags("Cooling").WithOpenApi();

            cooling.MapPost("/{sessionName}/post-cooling-values", (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.ReportPostCoolingValues command) =>
            LogPostCoolingValues(ctx, sessionName, command)).WithDescription("Stores the post-cooling values").WithName(nameof(LogPostCoolingValues));
        }

        private IResult LogPostCoolingValues(HttpContext ctx, string sessionName, BrewSessionActor.Commands.ReportPostCoolingValues command)
        {
            if (command.Validate(ctx) is IEnumerable<ModelError> errors) return Results.UnprocessableEntity(errors);

            _actorSystem.TellBrewSession(sessionName, command);

            return Results.Ok();
        }
    }
}
