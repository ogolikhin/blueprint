using ServiceLibrary.Models;
using System.Threading.Tasks;
using ServiceLibrary.Repositories.InstanceSettings;

namespace ServiceLibrary.Repositories
{
    public class SqlInstanceSettingsRepositoryMock: IInstanceSettingsRepository
    {
        private readonly EmailSettings _mockEmailSettings;
        private readonly Models.InstanceSettings _instanceSettings;

        public SqlInstanceSettingsRepositoryMock(EmailSettings emailSettings,
            Models.InstanceSettings instanceSettings) {
            _mockEmailSettings = emailSettings;
            _instanceSettings = instanceSettings;
            }

        public async Task<EmailSettings> GetEmailSettings()
        {
            return await Task.FromResult(_mockEmailSettings);
        }

        public async Task<Models.InstanceSettings> GetInstanceSettingsAsync(int maxInvalidLogonAttempts)
        {
            return await Task.FromResult(_instanceSettings);
        }
    }
}
