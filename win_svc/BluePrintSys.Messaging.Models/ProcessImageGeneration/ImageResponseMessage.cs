using NServiceBus;

namespace BluePrintSys.Messaging.Models.ProcessImageGeneration
{
    public class ImageResponseMessage : IMessage
    {
        public byte[] ProcessImage { get; set; }
    }
}
