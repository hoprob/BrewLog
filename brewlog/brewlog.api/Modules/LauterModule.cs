using Akka.Actor;
using brewlog.api.Extentions;
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

            lauter.MapPost("/{sessionName}/water-in-lauter", (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.AddTotalWaterInLauter command) =>
            TotalWaterInLauter(ctx, sessionName, command)).WithDescription("Stores the total water added in lauter stage").WithName(nameof(TotalWaterInLauter));
        }

        private IResult TotalWaterInLauter(HttpContext ctx, string sessionName, BrewSessionActor.Commands.AddTotalWaterInLauter command)
        {
            if (command.Validate(ctx) is IEnumerable<ModelError> errors) return Results.UnprocessableEntity(errors);

            _actorSystem.TellBrewSession(sessionName, command);

            return Results.Ok();
        }
    }
}
