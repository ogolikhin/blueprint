using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ArtifactStore.Services.VersionControl;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Models.VersionControl;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class VersionControlController : LoggableApiController
    {
        private readonly IVersionControlService _versionControlService;

        public override string LogSource { get; } = "ArtifactStore.VersionControl";

        public VersionControlController() : this(new VersionControlService())
        {
            
        }

        public VersionControlController(IVersionControlService versionControlService)
        {
            _versionControlService = versionControlService;
        }

        [HttpPost]
        [Route("artifacts/publish"), SessionRequired]
        [ActionName("PublishArtifacts")]
        [ResponseType(typeof(object))]
        public async Task<IHttpActionResult> PublishArtifacts([FromBody] IEnumerable<int> artifactIds, [FromUri] bool? all = null)
        {
            return Ok(await _versionControlService.PublishArtifacts( new PublishParameters
            {
                All = all,
                ArtifactIds = artifactIds
            }));
        }
    }
}