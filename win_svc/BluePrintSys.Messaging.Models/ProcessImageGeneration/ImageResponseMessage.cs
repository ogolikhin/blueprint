using NServiceBus;

namespace BluePrintSys.Messaging.Models.ProcessImageGeneration
{
    public class ImageResponseMessage : IMessage
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public byte[] ProcessImage { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
}
}
