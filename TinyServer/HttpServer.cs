using System;
using System.Net;
using log4net;

namespace TinyServer
{
    public class HttpServer : IServletFactory
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HttpServer));

        private readonly IConnector connector;

        /**
         * Storage for file resources. This HTTP server just keep its reference but actual implementation is
         * created outside this server scope.
         */
        private readonly IStorage storage;

        private readonly IContainer container;

        private readonly IEventsManager eventsManager;

        public HttpServer(IStorage storage, IPAddress listeningAddress, int listeningPort)
        {
            log.Debug("HttpServer(IStorage storage, IPAddress listeningAddress, int listeningPort)");
            this.connector = new HttpConnector(this, listeningAddress, listeningPort);
            this.storage = storage;
            this.container = null;
            this.eventsManager = new EventsManager();
        }

        public HttpServer(IContainer container, IPAddress listeningAddress, int listeningPort)
        {
            log.Debug("HttpServer(IContainer container, IPAddress listeningAddress, int listeningPort)");
            this.connector = new HttpConnector(this, listeningAddress, listeningPort);
            this.storage = null;
            this.container = container;
            this.eventsManager = new EventsManager();
        }

        public HttpServer(IStorage storage, IContainer container, IPAddress listeningAddress, int listeningPort)
        {
            log.Debug("HttpServer(IStorage storage, IContainer container, IPAddress listeningAddress, int listeningPort)");
            this.connector = new HttpConnector(this, listeningAddress, listeningPort);
            this.storage = storage;
            this.container = container;
            this.eventsManager = new EventsManager();
        }

        public void Start()
        {
            connector.Start();
            log.Info("HTTP server started.");
        }

        public void Stop()
        {
            connector.Stop();
            log.Info("HTTP server stopped.");
        }

        public void PushEvent(IEvent pushEvent)
        {
            eventsManager.PushEvent(pushEvent);
        }

        public IServlet CreateServlet(RequestType requestType)
        {
            log.Debug("CreateServlet(RequestType)");

            switch (requestType)
            {
                case RequestType.FILE:
                    if (storage == null)
                    {
                        throw new InvalidOperationException("File request but document root not defined.");
                    }
                    return new FileServlet(storage);

                case RequestType.RMI:
                    if (container == null)
                    {
                        throw new InvalidOperationException("HTTP-RMI request but container not defined.");
                    }
                    return new RmiServlet(container);

                case RequestType.EVENTS:
                    return new EventsServlet(eventsManager);

                default:
                    throw new NotSupportedException("Not supported request type " + requestType);
            }
        }
    }
}
