namespace TinyServer
{
    public interface IStorage
    {
        IResource GetResource(string requestURI);
    }
}
