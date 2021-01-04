using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyServer
{
    public interface IEventsManager
    {
        // Note: implementation should guarantee order; returned blocking collection should be backed by concurent queue
        BlockingCollection<IEvent> AcquireQueue(int eventsServletID);

        void ReleaseQueue(int eventsServletID);

        void PushEvent(IEvent pushEvent);
    }
}