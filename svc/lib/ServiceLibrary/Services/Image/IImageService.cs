using System.Net.Http;

namespace ServiceLibrary.Services.Image
{
    public interface IImageService
    {
        ImageType GetImageType(byte[] image);

        ByteArrayContent CreateByteArrayContent(byte[] image);

        byte[] ConvertBitmapImageToPng(byte[] image, int width, int height);
    }
}
