using Akka.Actor;
using Akka.Hosting;
using brewlog.application.Actors;
using brewlog.application.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace brewlog.application.Extentions
{
    public static class ActorSystemExtensions //TODO move to application
    {
        public static async Task<BrewSessionsCoordinatorActor.Responses.GetAllBrewSessionsResponse> GetAllBrewSessions(this ActorSystem actorSystem)
        {
            var registry = ActorRegistry.For(actorSystem);

            return await registry.Get<BrewSessionsCoordinatorActor>().Ask<BrewSessionsCoordinatorActor.Responses.GetAllBrewSessionsResponse>(
                new BrewSessionsCoordinatorActor.Queries.GetAllBrewSessions());
        }
        public static async Task<BrewSessionsCoordinatorActor.Responses.StartNewBrewSessionResponse> StartNewBrewSession(this ActorSystem actorSystem, string sessionName)
        {
            var registry = ActorRegistry.For(actorSystem);

            return await registry.Get<BrewSessionsCoordinatorActor>().Ask<BrewSessionsCoordinatorActor.Responses.StartNewBrewSessionResponse>(
                new BrewSessionsCoordinatorActor.Commands.StartNewBrewSession(sessionName));
        }

        public static void TellBrewSession(this ActorSystem actorSystem, string sessionName, object message)
        {
            var registry = ActorRegistry.For(actorSystem);

            registry.Get<BrewSessionsCoordinatorActor>().Tell(new BrewSessionsCoordinatorActor.Commands.SendMessageToBrewSession(sessionName, message));
        }

        public static async Task<AskBrewSessionResponse<T>> AskBrewSession<T>(this ActorSystem actorSystem, string sessionName, object message)
            where T : class, IBrewSessionResponse
        {
            var registry = ActorRegistry.For(actorSystem);

            var response = await registry.Get<BrewSessionsCoordinatorActor>().Ask<IBrewSessionResponse>(
                new BrewSessionsCoordinatorActor.Commands.SendMessageToBrewSession(sessionName, message));

            var typedResponse = response as T;

            return new AskBrewSessionResponse<T>(typedResponse, response.ErrorMessage ?? (typedResponse == null ? "Incorrect response" : null));
        }

        public static void Tell(this ICanTell target, object message)
        {
            target.Tell(message, ActorCell.GetCurrentSelfOrNoSender());
        }
    }

    public record AskBrewSessionResponse<T>(T? Response, string? ErrorMessage)
    {
        [MemberNotNullWhen(true, nameof(AskBrewSessionResponse<T>.Response))]
        public bool Success => Response != null && string.IsNullOrEmpty(ErrorMessage);
    }

}
