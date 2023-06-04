using Akka.Actor;
using brewlog.api.Extentions;
using brewlog.application.Actors;
using brewlog.application.Extentions;
using Carter;
using Carter.ModelBinding;
using Microsoft.AspNetCore.Mvc;

namespace brewlog.api.Modules
{
    public class MashModule : ICarterModule
    {
        private readonly ActorSystem _actorSystem;
        public MashModule(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var mash = app.MapGroup("/mash").WithTags("Mash").WithOpenApi();

            mash.MapGet("/{sessionName}/lactic-acid", async (HttpContext ctx, string sessionName) => await GetPhLoweringAcid(ctx, sessionName))
                .WithDescription("Gets suggested ml of 80% lactic acid for mash addition in order to get to target mash pH. Based on last reported mash pH.").WithName(nameof(GetPhLoweringAcid));

            mash.MapPost("/{sessionName}/report-ph", async (HttpContext ctx, string sessionName, [FromBody]BrewSessionActor.Commands.AddPhValue command) => await ReportMashPh(ctx, sessionName, command))
                .WithDescription("Stores the current measured mash pH").WithName(nameof(ReportMashPh));

            mash.MapPost("/{sessionName}/mash-complete", async (string sessionName) => await MashComplete(sessionName))
                .WithDescription("Completes the mash stage and moves to lautering stage.").WithName(nameof(MashComplete));
        }

      

        private async Task<IResult> GetPhLoweringAcid(HttpContext ctx, string sessionName)
        {
            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            try
            {
                var response = await sessionActor.Ask<BrewSessionActor.Responses.GetPhLoweringAcidResponse>(
                    new BrewSessionActor.Queries.GetPhLoweringAcid());

                return Results.Ok(response.MlLacticAcid);
            }
            catch (Exception ex)
            {
                return ex.ApiActorResponse();
            };
        }

        private async Task<IResult> ReportMashPh(HttpContext ctx, string sessionName, BrewSessionActor.Commands.AddPhValue command)
        {
            var validation = ctx.Request.Validate(command);
            if (!validation.IsValid) return Results.UnprocessableEntity(validation.GetFormattedErrors());

            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            sessionActor.Tell(command);

            return Results.Ok();
        }

        private async Task<IResult> MashComplete(string sessionName)
        {
            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            sessionActor.Tell(new BrewSessionActor.Commands.MashStageComplete());

            return Results.Ok();
        }
    }
}
