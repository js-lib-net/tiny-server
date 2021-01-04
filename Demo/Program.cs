using log4net.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyServer;

namespace Demo
{
    public class Service
    {
        public string Hello(string user)
        {
            return $"Hello {user}!";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            IStorage storage = new FileStorage("D:/runtime/kids-cademy/webapps/site/");
            IContainer container = new RmiContainer();
            container.AddMapping("Demo.Service", typeof(Service));

            HttpServer server = new HttpServer(storage, container, 8888);
            server.Start();

            Console.Read();
        }
    }
}
