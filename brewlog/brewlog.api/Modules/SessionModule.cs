using Akka.Actor;
using brewlog.api.Extentions;
using brewlog.application.Actors;
using brewlog.application.Extentions;
using Carter;
using Carter.ModelBinding;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace brewlog.api.Modules
{
    public class SessionModule : ICarterModule
    {





        private readonly ActorSystem _actorSystem;
        public SessionModule(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var session = app.MapGroup("/session").WithTags("Session").WithOpenApi();

            session.MapGet("/{sessionName}/state", async (string sessionName) => await GetSessionState(sessionName)).WithDescription("Gets the current session state").WithName(nameof(GetSessionState));

            session.MapGet("/{sessionName}/values", async (string sessionName) => await GetSessionValues(sessionName)).WithDescription("Gets the current values of the session").WithName(nameof(GetSessionValues));

            session.MapPost("/new", CreateNewSession).WithDescription("Creates a new session and returns the sessionActorPath").WithName(nameof(CreateNewSession));

            session.MapPost("/{sessionName}/recipe", async (string sessionName, HttpContext ctx, [FromBody] BrewSessionActor.Commands.AddSessionRecipe command) =>
            await AddSessionRecipe(ctx, sessionName, command)).WithDescription("Stores the recipe of the session").WithName(nameof(AddSessionRecipe));
        }

        private async Task<IResult> CreateNewSession([FromQuery] string sessionName)
        {
            await _actorSystem.GetBrewSession(sessionName);

            return Results.Ok(sessionName);
        }

        private async Task<IResult> GetSessionState(string sessionName)
        {
            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            try
            {
                var response = await sessionActor.Ask<BrewSessionActor.Responses.GetBrewSessionStateResponse>(
                    new BrewSessionActor.Queries.GetBrewSessionState());

                return Results.Ok(response.State);
            }
            catch (Exception ex)
            {
                return ex.ApiActorResponse();
            };
        }

        private async Task<IResult> GetSessionValues(string sessionName)
        {
            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            try
            {
                var response = await sessionActor.Ask<BrewSessionActor.Responses.BrewSessionValuesResponse>(
                    new BrewSessionActor.Queries.GetBrewSessionValues());

                return Results.Ok(response.SessionValues);
            }
            catch (Exception ex)
            {
                return ex.ApiActorResponse();
            };
        }

        private async Task<IResult> AddSessionRecipe(HttpContext ctx, string sessionName,
            BrewSessionActor.Commands.AddSessionRecipe command)
        {
            var validation = ctx.Request.Validate(command);
            if (!validation.IsValid)
                return Results.UnprocessableEntity(validation.GetFormattedErrors());

            var sessionActor = await _actorSystem.GetBrewSession(sessionName);

            try
            {
                var response = sessionActor.Ask<BrewSessionActor.Responses.AddSessionRecipeResponse>(command);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return ex.ApiActorResponse();
            };
        }
    }
}
