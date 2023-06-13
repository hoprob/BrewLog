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

            session.MapGet("/sessions", GetSessions).WithDescription("Returns all existing sessions from database").WithName(nameof(GetSessions));

            session.MapGet("/{sessionName}/log-notes", async (string sessionName) => await GetSessionLogNotes(sessionName))
                .WithDescription("Gets the log notes of the session.").WithName(nameof(GetSessionLogNotes));

            session.MapGet("/{sessionName}/state", async (string sessionName) => await GetSessionState(sessionName))
                .WithDescription("Gets the current session state").WithName(nameof(GetSessionState));

            session.MapGet("/{sessionName}/values", async (string sessionName) => await GetSessionValues(sessionName))
                .WithDescription("Gets the current values of the session").WithName(nameof(GetSessionValues));

            session.MapPost("/new", CreateNewSession).
                WithDescription("Creates a new session and returns the sessionActorPath").WithName(nameof(CreateNewSession));

            session.MapPost("/{sessionName}/recipe", async (string sessionName, HttpContext ctx, [FromBody] BrewSessionActor.Commands.AddSessionRecipe command) =>
            await AddSessionRecipe(ctx, sessionName, command)).WithDescription("Stores the recipe of the session").WithName(nameof(AddSessionRecipe));

            session.MapPost("/{sessionName}/log-note", (HttpContext ctx, string sessionName, [FromBody] BrewSessionActor.Commands.AddLogNote command) =>
            AddLogNote(ctx, sessionName, command)).WithDescription("Add log note to session").WithName(nameof(AddLogNote));
        }

        private async Task<IResult> GetSessions()
        {
            var response = await _actorSystem.GetAllBrewSessions();

            return response.Success ? Results.Ok(response.BrewSessions) : Results.BadRequest("Could not get the sessions from database.");
        }

        private async Task<IResult> GetSessionLogNotes(string sessionName)
        {
            var response = await _actorSystem.AskBrewSession<BrewSessionActor.Responses.GetLogNotesResponse>(sessionName, new BrewSessionActor.Queries.GetLogNotes());

            return response.Success ?  Results.Ok(response.Response.LogNotes) : Results.BadRequest(response.ErrorMessage);
        }

        private async Task<IResult> CreateNewSession([FromQuery] string sessionName)
        {
            var result = await _actorSystem.StartNewBrewSession(sessionName);

            return result.Success ? Results.Ok(result.SessionActor) : Results.BadRequest(result.ErrorMessage);
        }

        private async Task<IResult> GetSessionState(string sessionName)
        {
            var response = await _actorSystem.AskBrewSession<BrewSessionActor.Responses.GetBrewSessionStateResponse>(sessionName, new BrewSessionActor.Queries.GetBrewSessionState());

            return response.Success ? Results.Ok(response.Response.State) : Results.BadRequest(response.ErrorMessage);
        }

        private async Task<IResult> GetSessionValues(string sessionName)
        {
            var response = await _actorSystem.AskBrewSession<BrewSessionActor.Responses.BrewSessionValuesResponse>(sessionName,
                new BrewSessionActor.Queries.GetBrewSessionValues());

            return response.Success ? Results.Ok(response.Response.SessionValues) : Results.BadRequest(response.ErrorMessage);
        }

        private async Task<IResult> AddSessionRecipe(HttpContext ctx, string sessionName,
            BrewSessionActor.Commands.AddSessionRecipe command)
        {
            if(command.Validate(ctx) is IEnumerable<ModelError> errors) return Results.UnprocessableEntity(errors);

            var response = await _actorSystem.AskBrewSession<BrewSessionActor.Responses.AddSessionRecipeResponse>(sessionName, command);

            return response.Success ? Results.Ok() : Results.BadRequest(response.ErrorMessage);
        }

        private IResult AddLogNote(HttpContext ctx, string sessionName, BrewSessionActor.Commands.AddLogNote command)
        {
            if (command.Validate(ctx) is IEnumerable<ModelError> errors) return Results.UnprocessableEntity(errors);

             _actorSystem.TellBrewSession(sessionName, command);

            return Results.Ok();
        }
    }
}
