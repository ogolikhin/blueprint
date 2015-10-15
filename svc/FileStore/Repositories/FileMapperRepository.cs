using System;

namespace FileStore.Repositories
{
    public class FileMapperRepository : IFileMapperRepository
    {
        public string GetMappedOutputContentType(string fileType)
        {
            switch (fileType)
            {
                case ".txt":
                case "txt":
                    return "text/plain";
            }
            return string.Empty;
        }
    }
}