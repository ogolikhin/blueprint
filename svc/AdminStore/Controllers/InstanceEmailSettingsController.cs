using System.Threading.Tasks;
using System.Web.Http;
using AdminStore.Models.Emails;
using AdminStore.Services.Instance;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("instance/emailsettings")]
    [BaseExceptionFilter]
    public class InstanceEmailSettingsController : LoggableApiController
    {
        private readonly IEmailSettingsService _emailSettingsService;

        public InstanceEmailSettingsController() : this(new EmailSettingsService())
        {
            
        }

        public InstanceEmailSettingsController(IEmailSettingsService emailSettingsService)
        {
            _emailSettingsService = emailSettingsService;
        }

        public override string LogSource => "AdminStore.Instance.EmailSettings";

        [Route("sendtestemail")]
        [HttpPost, SessionRequired]
        [ActionName("SendTestEmail")]
        public Task SendTestEmailAsync([FromBody] EmailOutgoingSettings settings)
        {
            return _emailSettingsService.SendTestEmailAsync(Session.UserId, settings);
        }

    }
}
