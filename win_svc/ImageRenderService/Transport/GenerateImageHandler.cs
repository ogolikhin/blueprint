using System;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using BluePrintSys.Messaging.Models.ProcessImageGeneration;
using ImageRenderService.ImageGen;
using NServiceBus;

namespace ImageRenderService.Transport
{
    public class GenerateImageHandler : IHandleMessages<GenerateImageMessage>
    {
        public async Task Handle(GenerateImageMessage message, IMessageHandlerContext context)
        {

            ImageResponseMessage imageGenerated = null;
            try
            {
                //generate image
                var image = await ImageGenService.Instance.ImageGenerator.GenerateImageAsync(message.ProcessJsonModel, message.MaxWidth, message.MaxHeight, ImageFormat.Png);
                imageGenerated = new ImageResponseMessage
                {
                    ProcessImage = image.ToArray()
                };
            }
            catch (Exception ex)
            {
                imageGenerated = new ImageResponseMessage
                {
                    ProcessImage = null
                };
            }
           
            /*if (image == null)
            {
                await context.Reply(imageGenerated, options);
                //return Request.CreateResponse(HttpStatusCode.Conflict, "No browser available.");
            }*/

            var options = new ReplyOptions();
            await context.Reply(imageGenerated, options);
        }


       
    }
}
