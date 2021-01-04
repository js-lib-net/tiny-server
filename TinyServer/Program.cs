using System;

namespace TinyServer
{
    class Controller
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
            log4net.Config.BasicConfigurator.Configure();

            IStorage storage = new FileStorage("D:/runtime/kids-cademy/webapps/site/");
            IContainer container = null;
            HttpServer server = new HttpServer(storage, container, 8888);
            server.Start();

            Console.Read();
        }
    }
}
