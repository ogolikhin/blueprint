using System.Threading.Tasks;
using AdminStore.Models.Emails;
using ServiceLibrary.Models;

namespace AdminStore.Services.Instance
{
    public interface IEmailSettingsService
    {
        Task<EmailSettings> GetEmailSettingsAsync(int userId);

        Task SendTestEmailAsync(int userId, EmailOutgoingSettings outgoingSettings);
    }
}
