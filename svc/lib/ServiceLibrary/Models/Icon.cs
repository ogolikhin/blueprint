using System.Diagnostics.CodeAnalysis;

namespace ServiceLibrary.Models
{
    public class Icon
    {
        [SuppressMessage("Microsoft.Performance", "CA1819:Properties should not return arrays")]
        public byte[] Content { get; set; }

        public bool IsSvg { get; set; }
    }
}
