using Akka.Actor;
using brewlog.application.Actors;
using brewlog.application.Extentions;

namespace brewlog.api
{
    public static class ActorSystemExtensions
    {
        public static async Task<ICanTell> GetBrewSession(this ActorSystem actorSystem, string sessionName)
        {
            var sessionActor = actorSystem.ActorSelection($"akka://brewlogactorsystem/user/{sessionName}");

            if (await sessionActor.ActorExists())
                return sessionActor;

            return actorSystem.ActorOf<BrewSessionActor>(sessionName);
        }

        public static void Tell(this ICanTell target, object message)
        {
            target.Tell(message, ActorCell.GetCurrentSelfOrNoSender());
        }
    }
}
