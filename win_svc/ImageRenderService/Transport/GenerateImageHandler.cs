using System;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.ProcessImageGeneration;
using ImageRenderService.ImageGen;
using NServiceBus;

namespace ImageRenderService.Transport
{
    public class GenerateImageHandler : IHandleMessages<GenerateImageMessage>
    {
        public async Task Handle(GenerateImageMessage message, IMessageHandlerContext context)
        {
            Log.Info("Received Generate Image Message.");
            ImageResponseMessage imageGenerated;
            try
            {
                //generate image
                var image = await ImageGenService.Instance.ImageGenerator.GenerateImageAsync(message.ProcessJsonModel, message.MaxWidth, message.MaxHeight, ImageFormat.Png);
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
                        ErrorMessage = "Image generation failed.",
                        StackTrace = null
                    };
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
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
