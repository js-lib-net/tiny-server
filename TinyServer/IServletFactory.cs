namespace TinyServer
{
    interface IServletFactory
    {
        IServlet CreateServlet(RequestType requestType);
    }
}
