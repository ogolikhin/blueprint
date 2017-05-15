using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using ImageRenderService.ImageGen;

namespace ImageRenderService
{
    public class ImageController : ApiController
    {
     
        public async Task<HttpResponseMessage> Get()
        {
            //get parameters
            var formatStr = Request.GetQueryNameValuePairs().FirstOrDefault(p => p.Key.Equals("format")).Value;
            //set image format
            var format = formatStr == "jpeg" || formatStr == "jpg" ? ImageFormat.Jpeg : ImageFormat.Png;

            //generate image
            var processJsonModel = File.ReadAllText("ProcessData_Temp_200.json");
            var image = await ImageGenService.Instance.ImageGenerator.GenerateImageAsync(processJsonModel, 6000, 6000, format);
            if (image == null)
            {
                return Request.CreateResponse(HttpStatusCode.Conflict, "No browser available.");
            }
            var content = new StreamContent(image);

            image.Position = 0;

            //crate response
            var mimeType = format.Equals(ImageFormat.Jpeg) ? MimeMapping.GetMimeMapping(".jpeg") : MimeMapping.GetMimeMapping(".png");
            var response = Request.CreateResponse(HttpStatusCode.OK, true);


            response.Content = content;
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = $"test.{formatStr ?? "png"}"
            };
            response.Content.Headers.ContentLength = image.Length;

            return response;
        }


    }
}
