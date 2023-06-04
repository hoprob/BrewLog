using Akka.Actor;
using brewlog.application.Actors;
using brewlog.application.Extentions;
using Carter;
using Carter.ModelBinding;
using Microsoft.AspNetCore.Mvc;

namespace brewlog.api.Modules
{
    public class LauterModule : ICarterModule
    {
        private readonly ActorSystem _actorSystem;
        public LauterModule(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var lauter = app.MapGroup("/lauter").WithTags("Lauter").WithOpenApi();

            lauter.MapPost("/{sessionName}/water-in-lauter", async (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.AddTotalWaterInLauter command) =>
            await TotalWaterInLauter(ctx, sessionName, command)).WithDescription("Stores the total water added in lauter stage").WithName(nameof(TotalWaterInLauter));
        }

        private async Task<IResult> TotalWaterInLauter(HttpContext ctx, string sessionName, BrewSessionActor.Commands.AddTotalWaterInLauter command)
        {
            var validation = ctx.Request.Validate(command);
            if (!validation.IsValid) return Results.UnprocessableEntity(validation.GetFormattedErrors());

            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            sessionActor.Tell(command);

            return Results.Ok();
        }
    }
}
