using System;
using System.IO;

namespace TinyServer
{
    public class FileStorage : IStorage
    {
        private readonly string baseDir;

        public FileStorage(string baseDir)
        {
            this.baseDir = Path.GetFullPath(baseDir);
        }

        public IResource GetResource(string requestURI)
        {
            String filePath = Path.Combine(baseDir, requestURI.Substring(1));
            FileInfo fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException(filePath);
            }

            Stream stream = new BufferedStream(File.Open(filePath, FileMode.Open));
            // FileInfo.Extension starts with dot (.)
            ContentType contetType = ContentType.ForExtension(fileInfo.Extension.Substring(1));
            long contentLength = fileInfo.Length;
            return new Resource(stream, contetType, contentLength);
        }

        private class Resource : IResource
        {
            private readonly Stream stream;
            private readonly ContentType contentType;
            private readonly long contentLength;

            public Resource(Stream stream, ContentType contentType, long contentLength)
            {
                this.stream = stream;
                this.contentType = contentType;
                this.contentLength = contentLength;
            }

            public void Dispose()
            {
                stream.Dispose();
            }

            public long GetContentLength()
            {
                return contentLength;
            }

            public ContentType GetContentType()
            {
                return contentType;
            }

            public Stream GetInputStream()
            {
                return stream;
            }
        }
    }
}
