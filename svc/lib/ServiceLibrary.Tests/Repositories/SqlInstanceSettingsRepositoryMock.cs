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

        public Task<EmailSettings> GetEmailSettings()
        {
            return Task.FromResult(_mockEmailSettings);
        }

        public Task UpdateEmailSettingsAsync(EmailSettings settings)
        {
            return Task.FromResult(0);
        }

        public async Task<Models.InstanceSettings> GetInstanceSettingsAsync(int maxInvalidLogonAttempts)
        {
            return await Task.FromResult(_instanceSettings);
        }

        public async Task<int> CheckMaxArtifactsPerProjectBoundary(int projectId)
        {
            return await Task.FromResult(1);
        }
    }
}
