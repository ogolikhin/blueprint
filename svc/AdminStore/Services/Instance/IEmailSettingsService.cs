using System.Threading.Tasks;
using AdminStore.Models.Emails;
using ServiceLibrary.Models;

namespace AdminStore.Services.Instance
{
    public interface IEmailSettingsService
    {
        Task<EmailSettingsDto> GetEmailSettingsAsync(int userId);
        Task UpdateEmailSettingsAsync(int userId, EmailSettingsDto emailSettingsDto);

        Task SendTestEmailAsync(int userId, EmailOutgoingSettings outgoingSettings);
        Task TestIncomingEmailConnectionAsync(int userId, EmailIncomingSettings incomingSettings);
    }
}
