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

            //bottling.MapPost("/{sessionName}/storage-temperature", async (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.ChangeBottlingStorageTemperature command) =>
            //await SetStorageTemperature(ctx, sessionName, command)).WithDescription("Stores the storage temparature when carbonating").WithName(nameof(SetStorageTemperature));

            //bottling.MapPost("/{sessionName}/desired-co2-volume", async (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.SetDesiredCo2Colume command) =>
            //await SetDesiredCo2Volume(ctx, sessionName, command)).WithDescription("Stores the desired specified co2 volume").WithName(nameof(SetDesiredCo2Volume));

            bottling.MapPost("/{sessionName}/bottling-values", async (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.AddBottlingValues command) =>
            await AddBottlingValues(ctx, sessionName, command)).WithDescription("Stores values for carbonation").WithName(nameof(AddBottlingValues));
        }

       

        private async Task<IResult> GetNeededCo2PressureInPsi(string sessionName)
        {
            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            try
            {
                var response = await sessionActor.Ask<BrewSessionActor.Responses.GetCarbonationPressureInPsiRespone>
                (new BrewSessionActor.Queries.GetCarbonationPressureInPsi());

                return Results.Ok(response.psiPressure);
            }
            catch (Exception ex)
            {
                return ex.ApiActorResponse();
            };
        }

        //private async Task<IResult> SetStorageTemperature(HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.ChangeBottlingStorageTemperature command)
        //{
        //    var validation = ctx.Request.Validate(command);
        //    if (!validation.IsValid) return Results.UnprocessableEntity(validation.GetFormattedErrors());

        //    var sessionActor = await _actorSystem.GetBrewSession(sessionName);

        //    sessionActor.Tell(command);

        //    return Results.Ok();
        //}

        //private async Task<IResult> SetDesiredCo2Volume(HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.SetDesiredCo2Colume command)
        //{
        //    var validation = ctx.Request.Validate(command);
        //    if (!validation.IsValid) return Results.UnprocessableEntity(validation.GetFormattedErrors());

        //    var sessionActor = await _actorSystem.GetBrewSession(sessionName);

        //    sessionActor.Tell(command);

        //    return Results.Ok();
        //}

        private async Task<IResult> AddBottlingValues(HttpContext ctx, string sessionName, BrewSessionActor.Commands.AddBottlingValues command)
        {
            var validation = ctx.Request.Validate(command);
            if (!validation.IsValid) return Results.UnprocessableEntity(validation.GetFormattedErrors());

            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            sessionActor.Tell(command);

            return Results.Ok();
        }
    }
}
