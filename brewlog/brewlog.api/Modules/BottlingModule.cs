using Akka.Actor;
using brewlog.api.Extentions;
using brewlog.api.Validators;
using brewlog.application.Actors;
using brewlog.application.Extentions;
using Carter;
using Carter.ModelBinding;
using Microsoft.AspNetCore.Mvc;

namespace brewlog.api.Modules
{
    public class BottlingModule : ICarterModule
    {
        private readonly ActorSystem _actorSystem;
        public BottlingModule(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var bottling = app.MapGroup("/bottling").WithTags("Bottling").WithOpenApi();

            bottling.MapGet("/{sessionName}/co2-pressure-psi", async (string sessionName) => await GetNeededCo2PressureInPsi(sessionName))
                .WithDescription("Gets the needed co2 pressure in psi based on reported desired co2 volume and storage temperature").WithName(nameof(GetNeededCo2PressureInPsi));

            bottling.MapPost("/{sessionName}/bottling-values", (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.AddBottlingValues command) =>
            AddBottlingValues(ctx, sessionName, command)).WithDescription("Stores values for carbonation").WithName(nameof(AddBottlingValues));
        }

       

        private async Task<IResult> GetNeededCo2PressureInPsi(string sessionName)
        {
            var response = await _actorSystem.AskBrewSession<BrewSessionActor.Responses.GetCarbonationPressureInPsiRespone>
                (sessionName, new BrewSessionActor.Queries.GetCarbonationPressureInPsi());

            return response.Success ? Results.Ok(response.Response.psiPressure) : Results.BadRequest(response.ErrorMessage);
        }

        private IResult AddBottlingValues(HttpContext ctx, string sessionName, BrewSessionActor.Commands.AddBottlingValues command)
        {
            if (command.Validate(ctx) is IEnumerable<ModelError> errors) return Results.UnprocessableEntity(errors);

            _actorSystem.TellBrewSession(sessionName, command);

            return Results.Ok();
        }
    }
}
