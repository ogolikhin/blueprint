using System.Threading.Tasks;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories.InstanceSettings
{
    public interface IInstanceSettingsRepository
    {
        Task<EmailSettings> GetEmailSettings();
        Task UpdateEmailSettingsAsync(EmailSettings emailSettings);

        Task<Models.InstanceSettings> GetInstanceSettingsAsync(int maxInvalidLogonAttempts);
    }
}
