using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FileStore.Repositories
{
    public interface IPushStream
    {
        Task WriteToStream(Stream outputStream, HttpContent content, TransportContext context);
    }
}
