using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ServiceLibrary.Repositories.ConfigControl;
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
        public HttpContent Generate(int limitRecords = 0, bool showHeader = true)
        {
            try
            {
                using (var writer = new StreamWriter(_stream, Encoding.UTF8, 512, true))
                {
                    foreach (var line in _repository.GetLogEntries(limitRecords, showHeader))
                    {
                        writer.WriteLine(line);
                    }
                    writer.Flush();
                }
            }

            catch (Exception ex)
            {
                _log.LogError(WebApiConfig.LogRecordStatus, ex.Message).Wait();
                throw;
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