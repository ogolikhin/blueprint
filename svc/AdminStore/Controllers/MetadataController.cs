using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AdminStore.Services.Metadata;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Services.Image;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class MetadataController : LoggableApiController
    {
        private readonly IMetadataService _metadataService;
        private readonly IImageService _imageService;
        public override string LogSource => "AdminStore.Metadata";

        public MetadataController() : this(new MetadataService(), new ServiceLogRepository(), new ImageService())
        {
        }

        public MetadataController(IMetadataService metadataService,
            IServiceLogRepository log, IImageService imageService) : base(log)
        {
            _metadataService = metadataService;
            _imageService = imageService;
        }

        [HttpGet, ResponseCache(Duration = 86400)]
        [Route("icons"), SessionRequired]
        public async Task<HttpResponseMessage> GetIcons(string type, int? typeId = null, string color = null)
        {
            var httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);

            var icon = await _metadataService.GetIcon(type, typeId, color);
            if (icon == null)
            {
                throw new ResourceNotFoundException(String.Format(CultureInfo.CurrentCulture,
                    "artifact type {0}'s icon can not find", type));
            }
            httpResponseMessage.Content = _imageService.CreateByteArrayContent(icon.Content, icon.IsSvg);
            return httpResponseMessage;
        }
    }
}