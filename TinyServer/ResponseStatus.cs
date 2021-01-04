namespace TinyServer
{
    class ResponseStatus
    {
        /**
         * Status code (200) indicating the request succeeded normally.
         */
        public static readonly ResponseStatus OK = new ResponseStatus("200 OK");

        public static readonly ResponseStatus NO_CONTENT = new ResponseStatus("204 No Content");

        public static readonly ResponseStatus MOVED_PERMANENTLY = new ResponseStatus("301 Moved Permanently");

        /**
         * Status code (400) indicating the request sent by the client was syntactically incorrect.
         */
        public static readonly ResponseStatus BAD_REQUEST = new ResponseStatus("400 Bad request");

        /**
         * Status code (404) indicating that the requested resource is not available.
         */
        public static readonly ResponseStatus NO_FOUND = new ResponseStatus("404 Not found");

        /**
         * Status code (500) indicating an error inside the HTTP sync which prevented it from fulfilling the request.
         */
        public static readonly ResponseStatus INTERNAL_SERVER_ERROR = new ResponseStatus("500 Internal server error");

        private readonly string value;

        private ResponseStatus(string value)
        {
            this.value = value;
        }

        public string Value()
        {
            return value;
        }
    }
}
