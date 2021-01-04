using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyServer
{
    class DefaultStorage : IStorage
    {
        public IResource GetResource(string requestURI)
        {
            throw new NotImplementedException();
        }
    }
}
