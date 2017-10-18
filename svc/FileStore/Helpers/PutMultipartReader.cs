using System;
using System.IO;
using System.Threading.Tasks;
using FileStore.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace FileStore.Helpers
{
    public class PutMultipartReader : MultipartReader
    {
        private readonly Func<Stream, FileChunk, Task<long>> _function;
        private readonly FileChunk _fileChunk;
        private long? _fileSize;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileChunk"></param>
        /// <param name="function">Function to be executed</param>
        /// <param name="log"></param>
        public PutMultipartReader(Stream stream, FileChunk fileChunk, Func<Stream, FileChunk, Task<long>> function, IServiceLogRepository log) 
            : base(stream, log)
        {
            _function = function;
            _fileChunk = fileChunk;
        }
        
        protected override async Task ExecuteFunctionAsync(Stream stream)
        {
            await _log.LogVerbose(WebApiConfig.LogSourceFiles, "PUT: Posting first multi-part file chunk");
            _fileSize = await _function(stream, _fileChunk);
            await _log.LogVerbose(WebApiConfig.LogSourceFiles, $"PUT: Chunks posted {_fileChunk.ChunkNum - 1}");
        }

        public long? GetFileSize()
        {
            return _fileSize;
        }
    }
}