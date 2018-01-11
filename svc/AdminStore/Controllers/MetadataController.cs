using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Repositories.Metadata;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class MetadataController : LoggableApiController
    {
        private readonly MetadataService _metadataService;
        public override string LogSource => "AdminStore.Metadata";

        public MetadataController() : this(new MetadataService(), new ServiceLogRepository())
        {
        }

        public MetadataController(MetadataService metadataService,
            IServiceLogRepository log) : base(log)
        {
            _metadataService = metadataService;
        }

        [HttpGet]
        [Route("icons"), SessionRequired]
        public async Task<HttpResponseMessage> GetIcons(string type, int? typeId, string color)
        {
            ItemTypePredefined itemType;
            if (!string.IsNullOrEmpty(type) && !Enum.TryParse(type, true, out itemType))
            {
                throw new BadRequestException("Unknown item type");
            }

            var result = await _metadataService.GetCustomItemTypeIcon(typeId.GetValueOrDefault());

            var httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
            // httpResponseMessage.Content;
            return httpResponseMessage;
        }
    }
}