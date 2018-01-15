using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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

        [HttpGet, NoCache]
        [Route("icons"), SessionRequired]
        public async Task<HttpResponseMessage> GetIcons(string type, int? typeId)
        {
            ItemTypePredefined itemType = ItemTypePredefined.None;
            if (!string.IsNullOrEmpty(type) && !Enum.TryParse(type, true, out itemType))
            {
                throw new BadRequestException("Unknown item type");
            }


            var httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
            if (typeId == null)
            {
                var svgResult = _metadataService.GetItemTypeIcon(itemType);
                Console.WriteLine(svgResult.FirstOrDefault().ToString());
                httpResponseMessage.Content = new StringContent(svgResult.FirstOrDefault().ToString(), Encoding.UTF8, "application/xml");
                return httpResponseMessage;
            }

            var result = await _metadataService.GetCustomItemTypeIcon(typeId.GetValueOrDefault());

            httpResponseMessage.Content = result;
            return httpResponseMessage;
        }
    }
}