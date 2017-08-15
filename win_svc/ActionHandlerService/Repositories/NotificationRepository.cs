using ActionHandlerService.MessageHandlers.Notifications;
using ServiceLibrary.Models.Email;
using ServiceLibrary.Repositories;

namespace ActionHandlerService.Repositories
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
            new SmtpClient(smtpClientConfiguration).SendEmail(emailMessage);
        }
    }
}
