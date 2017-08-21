using System.Threading.Tasks;
using AdminStore.Models.Emails;

namespace AdminStore.Services.Instance
{
    public interface IEmailSettingsService
    {
        Task SendTestEmailAsync(int userId, EmailOutgoingSettings outgoingSettings);
        Task TestIncomingEmailConnectionAsync(int userId, EmailIncomingSettings incomingSettings);
    }
}
