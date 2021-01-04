using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace TinyServer
{
    public class Response
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Response));

        private static readonly string HTTP_VERSION = "HTTP/1.1";
        private static readonly string LWS = " ";
        private static readonly string CRLF = "\r\n";

        private readonly BufferedStream stream;

        private ResponseStatus status;
        private readonly IDictionary<string, string> headers = new Dictionary<string, string>();
        private bool commited;

        public Response(Stream stream)
        {
            this.stream = new BufferedStream(stream);
        }

        internal void SetStatus(ResponseStatus status)
        {
            this.status = status;
        }

        internal void SetHeader(string name, string value)
        {
            headers.Add(name, value);
        }

        internal void SetContentType(ContentType contentType)
        {
            headers.Add("Content-Type", contentType.Value());
        }

        internal void SetContentLength(long length)
        {
            headers.Add("Content-Length", length.ToString());
        }

        internal BufferedStream getOutputStream()
        {
            Commit();
            return stream;
        }

        internal bool IsCommitted()
        {
            return commited;
        }

        internal void Close()
        {
            try
            {
                stream.Flush();
            }
            catch (Exception e)
            {
                if (e.InnerException is SocketException)
                {
                    // most probably client ends connection and there is no way to flush stream data
                    // data is lost but is normal condition for current implementation of the server sent events
                }
                else
                {
                    log.Debug($"Fail to flush before close response: {e.Message}");
                }
            }
            try
            {
                stream.Close();
            }
            catch (Exception e)
            {
                // see flush comment
                if (!(e.InnerException is SocketException))
                {
                    log.Debug($"Fail to close response: {e.Message}");
                }
            }
        }

        /**
         * For response status and headers serialization used bytes stream.
         */
        private void Commit()
        {
            if (commited)
            {
                return;
            }
            commited = true;

            // write status line
            Write(HTTP_VERSION);
            Write(LWS);
            Write(status.Value());
            Write(CRLF);

            // write headers
            foreach (KeyValuePair<string, string> entry in headers)
            {
                Write(entry.Key);
                Write(": ");
                Write(entry.Value);
                Write(CRLF);
            }

            // write empty line to mark headers end
            Write(CRLF);
        }

        private void Write(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
