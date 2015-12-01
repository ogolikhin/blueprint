using System.Web;

namespace FileStore.Repositories
{
    public class FileMapperRepository : IFileMapperRepository
    {
        internal const string DefaultMediaType = "application/octet-stream";
        internal const string TextMediaType = "text/plain";
        internal const string RichTextMediaType = "text/richtext";
        internal const string PngMediaType = "image/png";
        internal const string JpgMediaType = "image/jpeg";
        internal const string BmpMediaType = "image/bmp";
        internal const string IefMediaType = "image/ief";

        internal const string SvgMediaType = "image/svg+xml";
        internal const string TiffMediaType = "image/tiff";
        internal const string CssMediaType = "text/css";
        internal const string HtmlMediaType = "text/html";

        internal const string RichTextFormatMediaType = "application/rtf";

        public string GetMappedOutputContentType(string fileType)
        {
            //http://webdesign.about.com/od/multimedia/a/mime-types-by-content-type.htm
            if (string.IsNullOrWhiteSpace(fileType))
            {
                return DefaultMediaType;
            }
            return MimeMapping.GetMimeMapping(fileType);
        }
    }
}
