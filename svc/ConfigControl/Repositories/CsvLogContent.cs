using ServiceLibrary.Repositories.ConfigControl;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ConfigControl.Repositories
{
    public class CsvLogContent : HttpContent
    {
        private const string dispositionType = "attachment";
        private const string attachmentFileName = "AdminStore.csv";
        private const string mediaType = "text/csv";

        private readonly IServiceLogRepository _log;
        private readonly ILogRepository _repository;

        private readonly MemoryStream _stream = new MemoryStream();

        public CsvLogContent() : this(new LogRepository(), new ServiceLogRepository())
        {
        }

        internal CsvLogContent(ILogRepository repository, IServiceLogRepository log)
        {
            _repository = repository;
            _log = log;

        }
        public HttpContent Generate(int recordLimit, long? recordId = null, int? chunkSize = null, bool showHeader = true)
        {
            try {
                using (var writer = new StreamWriter(_stream, Encoding.Default, 512, true))
                {
                    writer.AutoFlush = true;
                    try
                    {
                        if (!chunkSize.HasValue || chunkSize.Value < 0 || chunkSize.Value > recordLimit)
                            chunkSize = recordLimit;
                        int? totalRecords = 0;
                        long currentId = 0;
                        do
                        {
                            foreach (var record in _repository.GetRecords(chunkSize.Value, recordId, showHeader && currentId == 0))
                            {
                                currentId = record.Id;
                                writer.WriteLine(record.Line);
                            }
                            // remember the last id to start the next chunk request with that id-1
                            recordId = currentId - 1;
                            totalRecords += chunkSize;
                        } while (totalRecords < recordLimit);
                    }
                    catch (Exception ex)
                    {
                        writer.WriteLine(ex.Message);
                        writer.WriteLine(ex.StackTrace);
                        throw;
                    }
                    finally
                    {
                        _repository.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(WebApiConfig.LogRecordStatus, ex.Message).Wait();
            }
            finally
            {
                _stream.Position = 0;
            }
            Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            Headers.ContentDisposition = new ContentDispositionHeaderValue(dispositionType) { FileName = attachmentFileName };
            Headers.ContentLength = _stream.Length;
            return this;
        }
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return _stream.CopyToAsync(stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _stream.Length;
            return true;
        }


        protected override void Dispose(bool disposing)
        {
            if (_stream != null)
                _stream.Dispose();
            base.Dispose(disposing);

        }
    }

}