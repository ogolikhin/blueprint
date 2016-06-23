using System;
using System.Net;

namespace FileStore.Models
{
    internal class UploadResult
    {
        public Guid? FileId { get; set; }
        public HttpStatusCode Status { get; set; }
    }
}