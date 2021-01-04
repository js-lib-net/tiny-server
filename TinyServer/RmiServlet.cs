using log4net;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TinyServer
{
    class RmiServlet : IServlet
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RmiServlet));

        private readonly IContainer container;
        private readonly Json json;

        public RmiServlet(IContainer container)
        {
            log.Debug("RmiServlet(IContainer)");
            this.container = container;
            json = new Json();
        }

        public void Service(Request request, Response response)
        {
            log.Debug("Service(Request,Response)");

            string requestURI = request.GetRequestURI();
            int pathSeparatorIndex = requestURI.LastIndexOf('/');
            int extensionSeparatorIndex = requestURI.LastIndexOf('.');
            string typeName = TypeName(requestURI.Substring(0, pathSeparatorIndex));
            ++pathSeparatorIndex;
            string methodName = requestURI.Substring(pathSeparatorIndex, extensionSeparatorIndex - pathSeparatorIndex);

            Type type = container.GetMappedType(typeName);
            if (type == null)
            {
                throw new FileNotFoundException($"Type {typeName} not found.");
            }

            MethodInfo method = type.GetMethod(methodName);
            if (method == null)
            {
                throw new FileNotFoundException($"Method {typeName}#{methodName} not found.");
            }
            
            Type[] parameterTypes = method.GetParameters().Select(parameterInfo => parameterInfo.ParameterType).ToArray();
            object[] arguments = GetArguments(request, parameterTypes);
            Type returnType = method.ReturnType;

            object value = method.Invoke(container.GetInstance(type), arguments);

            response.SetHeader("Connection", "close");
            if (returnType == typeof(void))
            {
                response.SetStatus(ResponseStatus.NO_CONTENT);
                response.SetContentLength(0L);
                return;
            }

            byte[] body = Encoding.UTF8.GetBytes(json.Stringify(value));
            response.SetStatus(ResponseStatus.OK);
            response.SetContentType(ContentType.APPLICATION_JSON);
            response.SetContentLength(body.Length);
            response.getOutputStream().Write(body, 0, body.Length);
        }

        private object[] GetArguments(Request request, Type[] parameterTypes)
        {
            if (parameterTypes.Length == 0)
            {
                return new object[0];
            }
            byte[] body = new byte[request.GetContentLength()];
            request.GetInputStream().Read(body, 0, body.Length);
            return json.parse(Encoding.UTF8.GetString(body), parameterTypes);
        }

        private static string TypeName(string typePath)
        {
            return typePath.Substring(1).Replace('/', '.');
        }
    }
}
