using System;

namespace TinyServer
{
    public enum RequestType
    {
        NONE, FILE, RMI, REST, EVENTS
    }

    class RequestTypeFactory
    {
        private static readonly MappingRecord[] MAPPINGS = new MappingRecord[]
        {
            new MappingRecord("*.rmi", RequestType.RMI),
            new MappingRecord("/rest/*", RequestType.REST),
            new MappingRecord("/events/*", RequestType.EVENTS),
            new MappingRecord("*", RequestType.FILE)
        };

        public static RequestType ValueOf(Request request)
        {
            string requestURI = request.GetRequestURI();
            for (int i = 0; i < MAPPINGS.Length; ++i)
            {
                if (MAPPINGS[i].Match(requestURI))
                {
                    return MAPPINGS[i].requestType;
                }
            }
            return RequestType.NONE;
        }

        private struct MappingRecord
        {
            public string pattern;
            public RequestType requestType;
            public Func<string, string, bool> match;

            public MappingRecord(string pattern, RequestType requestType)
            {
                this.requestType = requestType;

                if (pattern.StartsWith("*"))
                {
                    this.pattern = pattern.Remove(0, 1);
                    match = EndsWith;
                }
                else if (pattern.EndsWith("*"))
                {
                    this.pattern = pattern.Remove(pattern.Length - 1);
                    match = StartsWith;
                }
                else
                {
                    this.pattern = null;
                    match = AcceptAll;
                }
            }

            public bool Match(string requestURI)
            {
                return match(requestURI, pattern);
            }

            private static bool AcceptAll(string requestURI, string pattern)
            {
                return true;
            }

            private static bool StartsWith(string requestURI, string pattern)
            {
                return requestURI.StartsWith(pattern);
            }

            private static bool EndsWith(string requestURI, string pattern)
            {
                return requestURI.EndsWith(pattern);
            }
        }
    }
}
