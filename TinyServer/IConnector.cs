using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyServer
{
    interface IConnector
    {
        void Start();

        void Stop();
    }
}
