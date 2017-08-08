using System.Threading.Tasks;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.InstanceSettings;

namespace ActionHandlerService.MessageHandlers.Notifications
{
    public class EmailSettingsRetriever
    {
        private readonly IInstanceSettingsRepository _instanceSettingsRepository;

        public EmailSettingsRetriever(string connectionString) : this(
            new SqlInstanceSettingsRepository(
                new SqlConnectionWrapper(connectionString)))
        {
            
        }

        public EmailSettingsRetriever(IInstanceSettingsRepository instanceSettingsRepository)
        {
            _instanceSettingsRepository = instanceSettingsRepository;
        }

        public async Task<EmailSettings> GetEmailSettings()
        {
            return await _instanceSettingsRepository.GetEmailSettings();
        }
    }
}
