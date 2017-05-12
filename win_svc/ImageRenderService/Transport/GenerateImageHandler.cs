using System.Drawing.Imaging;
using System.Threading.Tasks;
using CommonTransportModels;
using ImageRenderService.ImageGen;
using NServiceBus;

namespace ImageRenderService.Transport
{
    public class GenerateImageHandler : IHandleMessages<GenerateImageMessage>
    {
        public async Task Handle(GenerateImageMessage message, IMessageHandlerContext context)
        {
            var tempFile = @"C:\image.html";
            System.IO.File.WriteAllText(tempFile, message.SourceHtml);
            //generate image
            var image = await ImageGenService.Instance.ImageGenerator.GenerateImageAsync(tempFile, ImageFormat.Png);
            

            /*if (image == null)
            {
                await context.Reply(imageGenerated, options);
                //return Request.CreateResponse(HttpStatusCode.Conflict, "No browser available.");
            }*/

            var imageGenerated = new ImageResponseMessage
            {
                Result = image.ToArray()
            };

            var options = new ReplyOptions();
            await context.Reply(imageGenerated, options);
        }


       
    }
}
