using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyServer
{
    public interface IStorage
    {
        IResource GetResource(string requestURI);
    }
}
