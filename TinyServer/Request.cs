using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace TinyServer
{
    public class Request
    {
        private const char CR = '\r';
        private const int LF = '\n';
        private const int EOF = -1;

        /**
         * Bytes stream containing entire HTTP request: start line, headers, empty line separator and body.
         */
        private readonly BufferedStream stream;
        private readonly string remoteAddr;

        private RequestType requestType;
        private string requestURI;
        private readonly IDictionary<string, string> headers = new Dictionary<string, string>();
        private bool eof;

        public Request(Stream stream, string remoteAddr)
        {
            this.stream = new BufferedStream(stream);
            this.remoteAddr = remoteAddr;
        }

        internal void Parse()
        {
            // read start line and detect EOF or HTTP sync shutdown command
            string line = ReadLine();
            if (line == null)
            {
                eof = true;
                return;
            }

            int beginIndex = line.IndexOf(' ') + 1;
            int endIndex = line.LastIndexOf(' ');
            requestURI = line.Substring(beginIndex, endIndex - beginIndex);
            int separatorPosition = requestURI.LastIndexOf('?');
            if (separatorPosition != -1)
            {
                requestURI = requestURI.Substring(0, separatorPosition);
            }

            // TODO: update headers parser to consider values continuing on next line (starts with white space)
            while (!String.IsNullOrEmpty(line = ReadLine()))
            {
                int separatorIndex = line.IndexOf(':');
                headers.Add(line.Substring(0, separatorIndex).Trim(), line.Substring(separatorIndex + 1).Trim());
            }

            requestType = RequestTypeFactory.ValueOf(this);
        }

        private string ReadLine()
        {
            int c = stream.ReadByte();
            if (c == EOF)
            {
                return null;
            }
            StringBuilder line = new StringBuilder();
            bool foundCR = false;

            for (; ; )
            {
                switch (c)
                {
                    case EOF:
                        goto LINE_LOOP;

                    case CR:
                        foundCR = true;
                        break;

                    case LF:
                        if (foundCR)
                        {
                            goto LINE_LOOP;
                        }
                        line.Append(LF);
                        break;

                    default:
                        if (foundCR)
                        {
                            line.Append(CR);
                            foundCR = false;
                        }
                        line.Append((char)c);
                        break;
                }
                c = stream.ReadByte();
            }

        LINE_LOOP:
            return line.ToString();
        }

        internal bool HasHeader(string key)
        {
            return headers.ContainsKey(key);
        }

        internal string GetRequestURI()
        {
            return requestURI;
        }

        internal RequestType GetRequestType()
        {
            return requestType;
        }

        internal int GetContentLength()
        {
            headers.TryGetValue("Content-Length", out string value);
            return value != null ? Int32.Parse(value) : 0;
        }

        internal BufferedStream GetInputStream()
        {
            return stream;
        }

        internal string GetRemoteAddr()
        {
            return remoteAddr;
        }

        internal bool IsEof()
        {
            return eof;
        }

        internal void Close()
        {
            stream.Close();
        }

        internal string Dump()
        {
            StringBuilder builder = new StringBuilder();
            AddLine(builder, "Request-Type", requestType);
            AddLine(builder, "Request-URI", requestURI);

            foreach (KeyValuePair<string, string> entry in headers)
            {
                AddLine(builder, entry.Key, entry.Value);
            }
            return builder.ToString();
        }

        private static void AddLine(StringBuilder builder, String key, Object value)
        {
            builder.Append(key);
            builder.Append(": ");
            builder.Append(value?.ToString());
            builder.Append(Environment.NewLine);
        }
    }
}
