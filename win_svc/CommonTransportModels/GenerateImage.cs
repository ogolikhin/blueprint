using NServiceBus;

namespace CommonTransportModels
{
    public class GenerateImageMessage : IMessage
    {
        public string SourceHtml { get; set; }
    }

    public class ImageResponseMessage : IMessage
    {
        public byte[] Result { get; set; }
    }
}
