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

            mash.MapPost("/{sessionName}/report-ph", (HttpContext ctx, string sessionName, [FromBody]BrewSessionActor.Commands.AddPhValue command) => ReportMashPh(ctx, sessionName, command))
                .WithDescription("Stores the current measured mash pH").WithName(nameof(ReportMashPh));

            mash.MapPost("/{sessionName}/acid-addition", (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.AddAcidAddition command) => AcidAddition(ctx, sessionName, command))
                .WithDescription("Stores lactic acid addition").WithName(nameof(AcidAddition));
            
            mash.MapPost("/{sessionName}/mash-complete", (string sessionName) => MashComplete(sessionName))
                .WithDescription("Completes the mash stage and moves to lautering stage.").WithName(nameof(MashComplete));
        }


        private async Task<IResult> GetPhLoweringAcid(HttpContext ctx, string sessionName)
        {
            var response = await _actorSystem.AskBrewSession<BrewSessionActor.Responses.GetPhLoweringAcidResponse>(
                    sessionName, new BrewSessionActor.Queries.GetPhLoweringAcid());

            return response.Success ? Results.Ok(response.Response.MlLacticAcid) : Results.BadRequest(response.ErrorMessage);
        }

        private IResult ReportMashPh(HttpContext ctx, string sessionName, BrewSessionActor.Commands.AddPhValue command)
        {
            if (command.Validate(ctx) is IEnumerable<ModelError> errors) return Results.UnprocessableEntity(errors);

            _actorSystem.TellBrewSession(sessionName, command);

            return Results.Ok();
        }
        private IResult AcidAddition(HttpContext ctx, string sessionName, BrewSessionActor.Commands.AddAcidAddition command)
        {
            if (command.Validate(ctx) is IEnumerable<ModelError> errors) return Results.UnprocessableEntity(errors);

            _actorSystem.TellBrewSession(sessionName, command);

            return Results.Ok();
        }

        private IResult MashComplete(string sessionName)
        {
            _actorSystem.TellBrewSession(sessionName, new BrewSessionActor.Commands.MashStageComplete());

            return Results.Ok();
        }
    }
}
