using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TinyServer
{
    class EventsManager : IEventsManager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(EventsManager));

        private readonly IDictionary<int, BlockingCollection<IEvent>> queues = new Dictionary<int, BlockingCollection<IEvent>>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public BlockingCollection<IEvent> AcquireQueue(int eventsServletID)
        {
            if (queues.ContainsKey(eventsServletID))
            {
                throw new InvalidOperationException($"Queue for events servlet {eventsServletID} already created.");
            }
            BlockingCollection<IEvent> queue = new BlockingCollection<IEvent>(new ConcurrentQueue<IEvent>());
            queues.Add(eventsServletID, queue);
            log.Debug($"Acquired events queue {eventsServletID}.");
            return queue;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ReleaseQueue(int eventsServletID)
        {
            if (!queues.ContainsKey(eventsServletID))
            {
                throw new InvalidOperationException($"Missing queue for events servlet {eventsServletID}.");
            }
            queues.Remove(eventsServletID);
            log.Debug($"Released events queue {eventsServletID}.");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void PushEvent(IEvent pushEvent)
        {
            foreach (BlockingCollection<IEvent> queue in queues.Values)
            {
                queue.Add(pushEvent);
            }
        }
    }
}
