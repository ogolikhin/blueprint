using BlueprintSys.RC.Services.MessageHandlers.Notifications;
using ServiceLibrary.Models.Email;
using ServiceLibrary.Repositories;

namespace BlueprintSys.RC.Services.Repositories
{
    public interface INotificationRepository : IActionHandlerServiceRepository
    {
        void SendEmail(SMTPClientConfiguration smtpClientConfiguration, Message emailMessage);
    }

    public class NotificationRepository : ActionHandlerServiceRepository, INotificationRepository
    {
        public NotificationRepository(string connectionString) : base(connectionString)
        {
        }

        public NotificationRepository(ISqlConnectionWrapper connectionWrapper) : base(connectionWrapper)
        {
        }

        public NotificationRepository(ISqlConnectionWrapper connectionWrapper, IArtifactPermissionsRepository artifactPermissionsRepository) : base(connectionWrapper, artifactPermissionsRepository)
        {
        }

        public void SendEmail(SMTPClientConfiguration smtpClientConfiguration, Message emailMessage)
        {
            MailBee.Global.LicenseKey = "MN800-02CA3564CA2ACAAECAB17D4ADEC9-145F";
            new SmtpClient(smtpClientConfiguration).SendEmail(emailMessage);
        }
    }
}
