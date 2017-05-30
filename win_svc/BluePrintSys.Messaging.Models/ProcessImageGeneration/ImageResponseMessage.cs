using NServiceBus;

namespace BluePrintSys.Messaging.Models.ProcessImageGeneration
{
    [Express]
    public class ImageResponseMessage : IMessage
    {
        public byte[] ProcessImage { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
}
}
