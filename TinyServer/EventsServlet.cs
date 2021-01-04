using log4net;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace TinyServer
{
    public class EventsServlet : IServlet
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(EventsServlet));
        private const int KEEP_ALIVE_PERIOD = 30 * 1000;

        private static int ID_SEED;

        private IEventsManager eventsManager;
        private Json json;

        public EventsServlet(IEventsManager eventsManager)
        {
            log.Debug("EventsServlet(IEventsManager)");
            this.eventsManager = eventsManager;
            this.json = new Json();
        }

        public void Service(Request request, Response response)
        {
            log.Debug("Service(Request,Response)");

            response.SetStatus(ResponseStatus.OK);
            response.SetHeader("Cache-Control", "no-cache");
            response.SetHeader("Content-Type", ContentType.TEXT_EVENT_STREAM.Value());

            BufferedStream stream = response.getOutputStream();

            int id = ++ID_SEED;
            log.Debug($"Open event stream {id} from {request.GetRemoteAddr()}.");
            BlockingCollection<IEvent> queue = eventsManager.AcquireQueue(id);

            try
            {
                for (; ; )
                {
                    if (!queue.TryTake(out IEvent pushEvent, TimeSpan.FromMilliseconds(KEEP_ALIVE_PERIOD)))
                    {
                        pushEvent = new KeepAliveEvent();
                    }
                    log.Debug($"Sending event {pushEvent.GetType()} on stream {id}.");

                    try
                    {
                        // event: counterCRLF
                        Write(stream, "event:");
                        // event field is the simple type name of the push event instance
                        Write(stream, pushEvent.GetType().Name);
                        Write(stream, "\r\n");

                        // data: { json }CRLF
                        Write(stream, "data:");
                        Write(stream, json.Stringify(pushEvent));
                        Write(stream, "\r\n");

                        // empty line for event end mark
                        Write(stream, "\r\n");
                        stream.Flush();
                    }
                    catch (IOException e)
                    {
                        if (e.InnerException is SocketException) log.Debug($"Socket exception: {e.Message}");
                        log.Debug("Client closes event stream. Break Server-Sent events loop.");
                        break;
                    }
                }
            }
            finally
            {
                eventsManager.ReleaseQueue(id);
                log.Debug($"Close event stream {id} with {request.GetRemoteAddr()}.");
            }
        }

        private static void Write(Stream stream, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        private class KeepAliveEvent : IEvent
        {
        }
    }
}
