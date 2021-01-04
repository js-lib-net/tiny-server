using System;
using System.Collections.Generic;

namespace TinyServer
{
    public class RmiContainer : IContainer
    {
        private readonly IDictionary<string, Type> mappings = new Dictionary<string, Type>();

        private readonly IDictionary<Type, object> instances = new Dictionary<Type, object>();

        private readonly object instancesLock = new object();

        public void AddMapping(string typeName, Type type)
        {
            mappings.Add(typeName, type);
        }

        public Type GetMappedType(string typeName)
        {
            return mappings.TryGetValue(typeName, out Type type) ? type : null;
        }

        public object GetInstance(Type type)
        {
            if (!instances.TryGetValue(type, out object instance))
            {
                lock (instancesLock)
                {
                    if (!instances.TryGetValue(type, out instance))
                    {
                        instance = Activator.CreateInstance(type);
                        instances.Add(type, instance);
                    }
                }
            }
            return instance;
        }
    }
}
