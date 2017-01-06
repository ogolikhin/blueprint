using System.IO;

namespace ServiceLibrary.Models.Files
{
    public class File
    {
        public FileInfo Info { get; set; }

        public Stream ContentStream { get; set; }
    }
}
