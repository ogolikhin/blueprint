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
            switch (fileType.ToLower().Trim().TrimStart('.'))
            {
                case "txt":
                    return TextMediaType;
                case "rtx":
                    return RichTextMediaType;
                case "rtf":
                    return RichTextFormatMediaType;
                case "png":
                    return PngMediaType;
                case "jpg":
                case "jpeg":
                case "jpe":
                    return JpgMediaType;
                case "bmp":
                    return BmpMediaType;
                case "ief":
                    return IefMediaType;
                case "svg":
                    return SvgMediaType;
                case "tif":
                case "tiff":
                    return TiffMediaType;
                case "css":
                    return CssMediaType;
                case "htm":
                case "html":
                case "stm":
                    return HtmlMediaType;
            }
                    
            return DefaultMediaType;
        }
    }
}