using System;
using System.IO;
using System.Threading.Tasks;
using FileStore.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace FileStore.Helpers
{
    public class PostMultipartReader : MultipartReader
    {
        private DateTime? _expired;
        private FileChunk _fileChunk;
        private Func<string, string, Stream, DateTime?, Task<FileChunk>> _function;

        /// <summary>
        ///
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="expired"></param>
        /// <param name="function">Function to be executed</param>
        /// <param name="log"></param>
        public PostMultipartReader(Stream stream, DateTime? expired, Func<string, string, Stream, DateTime?, Task<FileChunk>> function, IServiceLogRepository log)
            : base(stream, log)
        {
            _expired = expired;
            _function = function;
        }

        protected override async Task ExecuteFunctionAsync(Stream stream)
        {
            // Gets current part's header information
            var fileName = MultipartPartParser.Filename.Replace("\"", string.Empty).Replace("%20", " ");
            var fileType = MultipartPartParser.ContentType;

            await _log.LogVerbose(WebApiConfig.LogSourceFiles, $"POST: Posting first multi-part file {fileName}");
            _fileChunk = await _function(fileName, fileType, MultipartPartParser, _expired);
            await _log.LogVerbose(WebApiConfig.LogSourceFiles, $"POST: Chunks posted {_fileChunk.ChunkNum - 1}");
        }

        public Guid? GetFileId()
        {
            return _fileChunk?.FileId;
        }
    }
}