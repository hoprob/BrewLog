﻿using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Persistence.EventStore.Query;
using EventStore.ClientAPI;

namespace Akka.Persistence.EventStore.Journal
{
    internal class EventStoreSubscriptions : IDisposable
    {
        private readonly IEventStoreConnection _conn;
        private IActorContext _context;

        private readonly Dictionary<IActorRef, ISet<EventStoreCatchUpSubscription>> _subscriptions = new();

        public EventStoreSubscriptions(IEventStoreConnection conn, IActorContext context)
        {
            _conn = conn;
            _context = context;
        }

        /// <summary>
        /// Subscribe
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="stream"></param>
        /// <param name="from">Zero-based exclusive offset, with null specifying beginning of stream</param>
        /// <param name="max"></param>
        /// <param name="resolved"></param>
        public void Subscribe(IActorRef subscriber, string stream, long? from, int max,
            Func<ResolvedEvent, object> resolved)
        {
            if (!_subscriptions.TryGetValue(subscriber, out var subscriptions))
            {
                subscriptions = new HashSet<EventStoreCatchUpSubscription>();
                _subscriptions.Add(subscriber, subscriptions);
                _context.WatchWith(subscriber, new Unsubscribe(stream, subscriber));
            }

            try
            {
                var self = _context.Self;

                // ES SubscribeToStreamFrom uses zero-based exclusive offset (lastCheckPoint)
                // Use null to specify from beginning of stream.
                var subscription = _conn.SubscribeToStreamFrom(
                    stream,
                    from,
                    new CatchUpSubscriptionSettings(max * 2, 500, false, true),
                    (sub, @event) =>
                    {
                        var p = resolved(@event);
                        if (p != null)
                            subscriber.Tell(p, self);
                    },
                    _ => subscriber.Tell(CaughtUp.Instance, self),
                    (_, reason, exception) =>
                    {
                        var msg = $"Subscription dropped due reason {reason.ToString()}";
                        subscriber.Tell(new SubscriptionDroppedException(msg, exception), self);
                    });


                subscriptions.Add(subscription);
            }
            catch (Exception)
            {
                if (subscriptions.Count == 0)
                {
                    _context.Unwatch(subscriber);
                }

                throw;
            }
        }

        public void Unsubscribe(string stream, IActorRef subscriber)
        {
            if (!_subscriptions.TryGetValue(subscriber, out var subscriptions)) return;
            var sub = subscriptions.FirstOrDefault(s => s.StreamId == stream);
            sub?.Stop();
            subscriptions.Remove(sub);
            if (subscriptions.Count == 0)
            {
                _context.Unwatch(subscriber);
            }
        }


        public void Dispose()
        {
            _context = null;
        }
    }
}