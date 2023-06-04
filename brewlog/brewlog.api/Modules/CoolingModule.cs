using Akka.Actor;
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

            cooling.MapPost("/{sessionName}/post-cooling-values", async (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.ReportPostCoolingValues command) =>
            await LogPostCoolingValues(ctx, sessionName, command)).WithDescription("Stores the post-cooling values").WithName(nameof(LogPostCoolingValues));
        }

        private async Task<IResult> LogPostCoolingValues(HttpContext ctx, string sessionName, BrewSessionActor.Commands.ReportPostCoolingValues command)
        {
            var validation = ctx.Request.Validate(command);
            if (!validation.IsValid) return Results.UnprocessableEntity(validation.GetFormattedErrors());

            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            sessionActor.Tell(command);

            return Results.Ok();
        }
    }
}
