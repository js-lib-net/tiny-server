using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace TinyServer
{
    class Json
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Json));

        public Json()
        {
            log.Debug("Json()");
        }

        public object[] parse(string json, Type[] types)
        {
            object[] objects = new object[types.Length];

            JArray parameters = JsonConvert.DeserializeObject<dynamic>(json);
            if (parameters == null)
            {
                log.Warn($"Could not load {objects.Length} argument(s) from request body. Arguments left not initialized.");
                return objects;
            }

            for (int i = 0; i < types.Length; ++i)
            {
                if (i == parameters.Count)
                {
                    log.Warn($"Required {objects.Length} argument(s) but found {parameters.Count}. Some arguments left not initialized.");
                    break;
                }
                objects[i] = parameters[i].ToObject(types[i]);
            }
            return objects;
        }

        internal string Stringify(object value)
        {
            return JsonConvert.SerializeObject(value);
        }
    }
}
