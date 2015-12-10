using System;
using System.IO;
using System.Threading.Tasks;
using FileStore.Models;

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
        public PostMultipartReader(Stream stream, DateTime? expired, Func<string,string,Stream,DateTime?, Task<FileChunk>> function)
            : base(stream)
        {
            _expired = expired;
            _function = function;
        }

        protected override void HandleMultipartReadError(string error)
        {
            throw new MultipartReadException(error);
        }

        protected override async Task ExecuteFunctionAsync(Stream stream)
        {
            // Gets current part's header information
            var fileName = MultipartPartParser.Filename.Replace("\"", string.Empty).Replace("%20", " ");
            var fileType = MultipartPartParser.ContentType;

            LogHelper.Log.DebugFormat("POST: Posting first multi-part file {0}", fileName);
            _fileChunk = await _function(fileName, fileType, MultipartPartParser, _expired);
            LogHelper.Log.DebugFormat("POST: Chunks posted {0}", _fileChunk.ChunkNum - 1);
        }
        
        public Guid? GetFileId()
        {
            return _fileChunk?.FileId;
        }
    }
}