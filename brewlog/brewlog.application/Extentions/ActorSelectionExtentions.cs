using Akka.Actor;
using System.Runtime.CompilerServices;

namespace brewlog.application.Extentions
{
    public static class ActorSelectionExtentions 
    {
        public static async Task<bool> ActorExists(this ActorSelection actor)
        {
            var identifySessionActor = await actor.Ask<ActorIdentity>(new Identify(null));

            return identifySessionActor.Subject is null ? false : true;
        }
    }
}
