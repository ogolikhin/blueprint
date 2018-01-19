using System.Collections.Generic;
using System.Net.Http;

namespace ServiceLibrary.Services.Image
{
    public interface IImageService
    {
        ImageType GetImageType(byte[] image);

        ByteArrayContent CreateByteArrayContent(IEnumerable<byte> image, bool isSvg);

        byte[] ConvertBitmapImageToPng(byte[] image, int width, int height);
    }
}
