using System;

namespace TinyServer
{
    public interface IContainer
    {
        void AddMapping(string typeName, Type type);

        Type GetMappedType(string typeName);

        object GetInstance(Type type);
    }
}
