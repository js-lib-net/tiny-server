using System.IO;
using log4net;

namespace TinyServer
{
    class FileServlet : IServlet
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FileServlet));

        private const int BUFFER_SIZE = 8192;

        private readonly IStorage storage;

        public FileServlet(IStorage storage)
        {
            log.Debug("FileServlet(IStorage)");
            this.storage = storage;
        }

        public void Service(Request request, Response response)
        {
            log.Debug("Service(Request,Response)");
            using (IResource resource = storage.GetResource(request.GetRequestURI()))
            {
                response.SetStatus(ResponseStatus.OK);
                response.SetContentType(resource.GetContentType());
                response.SetContentLength(resource.GetContentLength());

                BufferedStream responseStream = response.getOutputStream();
                byte[] buffer = new byte[BUFFER_SIZE];
                int length;
                while ((length = resource.GetInputStream().Read(buffer, 0, buffer.Length)) > 0)
                {
                    // log.Debug($"Buffer size: {length}.");
                    responseStream.Write(buffer, 0, length);
                }
            }
        }
    }
}
