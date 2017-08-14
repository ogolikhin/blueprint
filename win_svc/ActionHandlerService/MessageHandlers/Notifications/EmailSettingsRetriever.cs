using System.Threading.Tasks;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.InstanceSettings;

namespace ActionHandlerService.MessageHandlers.Notifications
{
    public class EmailSettingsRetriever
    {
        public async Task<EmailSettings> GetEmailSettings(IInstanceSettingsRepository instanceSettingsRepository)
        {
            return await instanceSettingsRepository.GetEmailSettings();
        }
    }
}
