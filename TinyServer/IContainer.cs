using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyServer
{
    public interface IContainer
    {
        void AddMapping(string typeName, Type type);

        Type GetMappedType(string typeName);

        object GetInstance(Type type);
    }
}
