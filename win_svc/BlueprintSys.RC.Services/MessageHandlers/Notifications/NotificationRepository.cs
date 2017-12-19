using ServiceLibrary.Models.Email;
using ServiceLibrary.Repositories;

namespace BlueprintSys.RC.Services.MessageHandlers.Notifications
{
    public interface INotificationRepository : IBaseRepository
    {
        void SendEmail(SMTPClientConfiguration smtpClientConfiguration, Message emailMessage);
    }

    public class NotificationRepository : BaseRepository, INotificationRepository
    {
        public NotificationRepository(string connectionString) : base(new SqlConnectionWrapper(connectionString))
        {
        }

        public void SendEmail(SMTPClientConfiguration smtpClientConfiguration, Message emailMessage)
        {
            MailBee.Global.LicenseKey = "MN800-02CA3564CA2ACAAECAB17D4ADEC9-145F";
            new SmtpClient(smtpClientConfiguration).SendEmail(emailMessage);
        }
    }
}
