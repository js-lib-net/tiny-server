using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyServer
{
    public interface IServlet
    {
        void Service(Request request, Response response);
    }
}
