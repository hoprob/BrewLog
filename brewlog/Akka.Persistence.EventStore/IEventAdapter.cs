﻿using EventStore.ClientAPI;

namespace Akka.Persistence.EventStore;

public interface IEventAdapter
{
    EventData Adapt(IPersistentRepresentation persistentMessage);
    
    IPersistentRepresentation? Adapt(ResolvedEvent @event);
}