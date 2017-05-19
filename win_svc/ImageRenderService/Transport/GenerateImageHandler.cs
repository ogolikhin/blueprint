using System;
using System.Drawing.Imaging;
using System.IO;
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

            ImageResponseMessage imageGenerated;
            MemoryStream image = null;
            try
            {
                //generate image
                image = await ImageGenService.Instance.ImageGenerator.GenerateImageAsync(message.ProcessJsonModel, message.MaxWidth, message.MaxHeight, ImageFormat.Png);
                imageGenerated = new ImageResponseMessage
                {
                    ProcessImage = image.ToArray(),
                    ErrorMessage = null,
                    StackTrace = null
                };

                if (image.Capacity == 0)
                {
                    imageGenerated = new ImageResponseMessage
                    {
                        ProcessImage = null,
                        ErrorMessage = "Image generation failed",
                        StackTrace = null
                    };
                }
            }
            catch (Exception ex)
            {
                imageGenerated = new ImageResponseMessage
                {
                    ProcessImage = null,
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace
                };
            }

            var options = new ReplyOptions();
            await context.Reply(imageGenerated, options);
        }
    }
}
