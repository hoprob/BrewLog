using Akka.Actor;
using System.Runtime.CompilerServices;

namespace brewlog.api.Extentions
{
    public static class ExceptionExtentions
    {
        public static IResult ApiActorResponse(this Exception ex) //TODO Better name...
        {
            if (ex is AskTimeoutException)
                return Results.BadRequest(new { Error = "Actor AskTimeoutExeption. Most likely, the command or query is not avalable in the Actor behavior", ExceptionMessage = ex.Message });
            else
                return Results.StatusCode(500);
        }
    }
}
