namespace FileStore.Repositories
{
    public class FileMapperRepository : IFileMapperRepository
    {
        internal const string DefaultMediaType = "application/octet-stream";

        public string GetMappedOutputContentType(string fileType)
        {
            //http://webdesign.about.com/od/multimedia/a/mime-types-by-content-type.htm
            if (string.IsNullOrWhiteSpace(fileType))
            {
                return DefaultMediaType;
            }
            switch (fileType.ToLower().TrimStart('.'))
            {
                case "txt":
                    return "text/plain";
                case "rtx":
                    return "text/richtext";
                case "png":
                    return "image/png";
                case "jpg":
                case "jpeg":
                case "jpe":
                    return "image/jpeg";
                case "bmp":
                    return "image/bmp";
                case "ief":
                    return "image/ief";
                case "svg":
                    return "image/svg+xml";
                case "tif":
                case "tiff":
                    return "image/tiff";
                case "css":
                    return "text/css";
                case "htm":
                case "html":
                case "stm":
                    return "text/html";
            }
                    
            return DefaultMediaType;
        }
    }
}