using Akka.Actor;
using Akka.Persistence;
using brewlog.application.Extentions;
using brewlog.application.Interfaces;
using brewlog.domain.Models;
using System.Collections.Immutable;
using System.Security.Cryptography.X509Certificates;

namespace brewlog.application.Actors
{
    public class BrewSessionsCoordinatorActor : ReceivePersistentActor
    {
        public static class Commands
        {
            public record StartNewBrewSession(string Name);
            public record SendMessageToBrewSession(string SessionName, object Message);
        }

        public static class Queries
        {
            public record GetAllBrewSessions;
        }

        public static class Events
        {
            public record NewBrewSessionStarted(DateTimeOffset CreatedAt, string Name);
        }

        public static class Responses
        {
            public record StartNewBrewSessionResponse(string SessionActor, string? ErrorMessage = null) : IBrewSessionResponse
            {
                public bool Success => String.IsNullOrEmpty(ErrorMessage);
            };
            public record GetAllBrewSessionsResponse(IImmutableList<BrewSession> BrewSessions, string? ErrorMessage = null) : IBrewSessionResponse
            {
                public bool Success => String.IsNullOrEmpty(ErrorMessage);
            };

            public record NoBrewSessionFoundResponse(string ErrorMessage) : IBrewSessionResponse
            {
                public NoBrewSessionFoundResponse() : this("No brew session found")
                {
                    
                }
            }
        }

        public override string PersistenceId => "brewsessionscoordinator";

        private ImmutableList<BrewSession> _brewSessions = ImmutableList<BrewSession>.Empty;

        public BrewSessionsCoordinatorActor()
        {
            Recover<Events.NewBrewSessionStarted>(On);

            Command<Commands.StartNewBrewSession>(cmd =>
            {
                if(!_brewSessions.Exists(x => x.SessionName == cmd.Name))
                {
                    var sessionActor = GetBrewSession(cmd.Name);
                    Persist(new Events.NewBrewSessionStarted(DateTimeOffset.Now, cmd.Name), On);
                    sessionActor.Tell(new BrewSessionActor.Commands.AddLogNote($"New session with name: {cmd.Name} started."));
                    Sender.Tell(new Responses.StartNewBrewSessionResponse(cmd.Name));
                }
                else
                {
                    Sender.Tell(new Responses.StartNewBrewSessionResponse(cmd.Name, "The name already exists as a session, try with another one."));
                }
            });

            Command<Commands.SendMessageToBrewSession>(cmd =>
            {
                if(_brewSessions.Exists(x => x.SessionName == cmd.SessionName))
                {
                    var brewSession = GetBrewSession(cmd.SessionName);

                    brewSession.Tell(cmd.Message, Sender);
                }
                else
                {
                    Sender.Tell(new Responses.NoBrewSessionFoundResponse());
                }
            });

            Command<Queries.GetAllBrewSessions>(_ => Sender.Tell(new Responses.GetAllBrewSessionsResponse(_brewSessions)));
        }

        public static Props Init()
        {
            return Props.Create(() => new BrewSessionsCoordinatorActor());
        }

        private void On(Events.NewBrewSessionStarted evnt)
        {
            _brewSessions = _brewSessions.Add(new BrewSession(evnt.CreatedAt, evnt.Name));
        }

        private static IActorRef GetBrewSession(string sessionName)
        {
            var brewSession = Context.Child(sessionName);

            if (Equals(brewSession, ActorRefs.Nobody))
                return Context.ActorOf<BrewSessionActor>(sessionName);
         
            return brewSession;
        }
    }
}
