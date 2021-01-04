namespace TinyServer
{
    public interface IServlet
    {
        void Service(Request request, Response response);
    }
}
