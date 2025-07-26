using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using log4net;

namespace TinyServer
{
    class HttpConnector : IConnector
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HttpConnector));

        private static readonly int RECEIVE_TIMEOUT = 8000;

        private readonly IServletFactory servletFactory;
        private readonly IPAddress listeningAddress;
        private readonly int listeningPort;
        private volatile bool running;

        public HttpConnector(IServletFactory servletFactory, IPAddress listeningAddress, int listeningPort)
        {
            log.Debug("HttpConnector(IServletFactory servletFactory, IPAddress listeningAddress, int listeningPort)");
            log.Debug($"listeningAddress: {listeningAddress}");
            log.Debug($"listeningPort: {listeningPort}");

            this.servletFactory = servletFactory;
            this.listeningAddress = listeningAddress;
            this.listeningPort = listeningPort;
        }

        public void Start()
        {
            log.Debug("Start()");
            Thread thread = new Thread(new ThreadStart(Run))
            {
                IsBackground = true
            };
            thread.Start();
        }

        public void Stop()
        {
            log.Debug("Stop()");
            running = false;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP))
            {
                try
                {
                    socket.Connect(IPAddress.Loopback, listeningPort);
                }
                catch (Exception e)
                {
                    log.Error(e);
                }
            }
        }

        public void Run()
        {
            log.Debug($"Start HTTP connector thread {Thread.CurrentThread}");

            // to listen to both IPv4 and IPv6 need to start listening to IPv6 and set socket option IPv6Only to false
            TcpListener serverSocket = new TcpListener(listeningAddress, listeningPort);
            serverSocket.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
            serverSocket.Start();
            log.Debug("Listen for HTTP requests on " + serverSocket.LocalEndpoint);

            running = true;
            for (; ; )
            {
                try
                {
                    Socket socket = serverSocket.AcceptSocket();
                    if (!running)
                    {
                        log.Debug("Got shutdown command. Break HTTP connector loop.");
                        break;
                    }
                    ThreadPool.QueueUserWorkItem((stateInfo) =>
                    {
                        Service(socket);
                    });
                }
                catch (Exception e)
                {
                    log.Error("Fatal error on HTTP request processing.", e);
                }
            }
        }

        private void Service(Socket socket)
        {
            string remoteAddress = socket.RemoteEndPoint.ToString();
            NDC.Push(remoteAddress);

            Request request = null;
            Response response = null;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                socket.ReceiveTimeout = RECEIVE_TIMEOUT;
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, false);

                request = new Request(new NetworkStream(socket, FileAccess.Read, false), remoteAddress);
                response = new Response(new NetworkStream(socket, FileAccess.Write, false));

                request.Parse();
                if (request.IsEof())
                {
                    log.Debug("Remote socket close. Close current connection and continue waiting for new ones.");
                    return;
                }

                IServlet servlet = servletFactory.CreateServlet(request.GetRequestType());
                servlet.Service(request, response);
            }
            catch (FileNotFoundException e)
            {
                log.Warn(e.Message);
                SendException(response, ResponseStatus.NO_FOUND, e);
            }
            catch (Exception e)
            {
                log.Error("Error processing request.", e);
                log.Warn(request.Dump());
                SendException(response, ResponseStatus.INTERNAL_SERVER_ERROR, e);
                NDC.Pop();
                return;
            }
            finally
            {
                request.Close();
                response?.Close();
                CloseSocket(socket);
            }

            // at this point request is guaranteed to be initialized
            // if request initialization fails for some reason there is exception that does return
            stopwatch.Stop();
            log.Info($"{request.GetRequestType()} {request.GetRequestURI()} processed in {stopwatch.ElapsedMilliseconds} msec.");
            NDC.Pop();
        }

        private static void CloseSocket(Socket socket)
        {
            // it is critical to call Shutdown(SocketShutdown.Send) before Close()
            // otherwise C# implentation execute RST on TCP connection instead of FIN

            // note that SocketShutdown.Receive or SocketShutdown.Both still execute RST

            try
            {
                socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception e)
            {
                log.Error(e);
            }
            socket.Close();
        }

        private static void SendException(Response response, ResponseStatus status, Exception exception)
        {
            if (response == null || response.IsCommitted())
            {
                log.Error("Attempt to send exception on null or commited response.");
                return;
            }
            response.SetStatus(status);

            byte[] stackTrace = Encoding.UTF8.GetBytes(Environment.StackTrace);
            response.SetContentType(ContentType.TEXT_PLAIN);
            response.SetContentLength(stackTrace.Length);

            try
            {
                response.getOutputStream().Write(stackTrace, 0, stackTrace.Length);
            }
            catch (IOException e)
            {
                log.Error(e);
            }
        }
    }
}
