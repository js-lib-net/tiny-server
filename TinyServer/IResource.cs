using System;
using System.IO;

namespace TinyServer
{
    public interface IResource : IDisposable
    {
        ContentType GetContentType();

        long GetContentLength();

        Stream GetInputStream();
    }
}
