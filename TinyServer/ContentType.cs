using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TinyServer
{
    public class ContentType
    {
        public static readonly ContentType NONE = new ContentType("");
        public static readonly ContentType TEXT_PLAIN = new ContentType("text/plain; charset=UTF-8");
        public static readonly ContentType TEXT_HTML = new ContentType("text/html; charset=UTF-8");
        public static readonly ContentType TEXT_CSS = new ContentType("text/css; charset=UTF-8");
        public static readonly ContentType TEXT_JS = new ContentType("text/javascript; charset=UTF-8");
        public static readonly ContentType TEXT_XML = new ContentType("text/xml; charset=UTF-8");
        public static readonly ContentType TEXT_CSV = new ContentType("text/csv; charset=UTF-8");
        public static readonly ContentType MULTIPART_FORM = new ContentType("multipart/form-data");
        public static readonly ContentType MULTIPART_MIXED = new ContentType("multipart/mixed");
        public static readonly ContentType URLENCODED_FORM = new ContentType("application/x-www-form-urlencoded; charset=UTF-8");
        public static readonly ContentType APPLICATION_JSON = new ContentType("application/json; charset=UTF-8");
        public static readonly ContentType APPLICATION_PDF = new ContentType("application/pdf");
        public static readonly ContentType APPLICATION_STREAM = new ContentType("application/octet-stream");
        public static readonly ContentType IMAGE_PNG = new ContentType("image/png");
        public static readonly ContentType IMAGE_JPEG = new ContentType("image/jpeg");
        public static readonly ContentType IMAGE_GIF = new ContentType("image/gif");
        public static readonly ContentType IMAGE_TIFF = new ContentType("image/tiff");
        public static readonly ContentType IMAGE_SVG = new ContentType("image/svg+xml");
        public static readonly ContentType TEXT_EVENT_STREAM = new ContentType("text/event-stream; charset=UTF-8");

        private readonly string value;
        private ContentType(string value)
        {
            this.value = value;
        }

        public string Value()
        {
            return value;
        }

        public static ContentType ForFilePath(String filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return ForExtension(fileInfo.Extension);
        }

        public static ContentType ForExtension(string extension)
        {
            switch (extension)
            {
                case "htm":
                case "html":
                    return ContentType.TEXT_HTML;

                case "png":
                    return ContentType.IMAGE_PNG;

                case "css":
                    return ContentType.TEXT_CSS;

                case "js":
                    return ContentType.TEXT_JS;

                case "jpg":
                case "jpeg":
                    return ContentType.IMAGE_PNG;

                case "gif":
                    return ContentType.IMAGE_GIF;

                default:
                    return ContentType.NONE;
            }
        }
    }
}
