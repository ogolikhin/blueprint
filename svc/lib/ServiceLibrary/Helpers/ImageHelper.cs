using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace ServiceLibrary.Helpers
{
    // Copied with small modifications from Raptor solution
    public class ImageHelper
    {
        // http://www.w3.org/TR/PNG/#5PNG-file-signature
        // The first eight bytes of a PNG datastream always contain the following (decimal) values:
        // 137 80 78 71 13 10 26 10
        private static readonly byte[] PNG_SIGNATURE = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private static readonly byte[] JPG_SIGNATURE = { 0xff, 0xd8 };

        public static ImageType GetImageType(byte[] image)
        {
            if (JPG_SIGNATURE.SequenceEqual(image.Take(JPG_SIGNATURE.Length)))
            {
                return ImageType.Jpg;
            }
            if (PNG_SIGNATURE.SequenceEqual(image.Take(PNG_SIGNATURE.Length)))
            {
                return ImageType.Png;
            }
            return ImageType.Unknown;
        }

        /// <summary>
        /// Creates HttpContent object and assignes content type.
        /// In case the method can not indicate a content type then it omits the type in order to allow the recipient to guess the type.
        /// http://tools.ietf.org/html/rfc7231#section-3.1.1.5
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static ByteArrayContent CreateByteArrayContent(byte[] image)
        {
            var byteArrayContent = new ByteArrayContent(image);
            var fileExtension = default(string);
            var imageType = GetImageType(image);
            switch (imageType)
            {
                case ImageType.Png:
                    fileExtension = ".png";
                    break;
                case ImageType.Jpg:
                    fileExtension = ".jpeg";
                    break;
            }
            if (fileExtension != null)
            {
                var mimeType = MimeMapping.GetMimeMapping(fileExtension);
                byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            }
            return byteArrayContent;
        }
    }

    public enum ImageType
    {
        Unknown,
        Png,
        Jpg
    }
}
