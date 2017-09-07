using ServiceLibrary.Models.Email;

namespace ServiceLibrary.Notification.Templates
{
    public partial class NotificationEmailContent
    {
        private INotificationEmail Email { get; }

        public NotificationEmailContent(INotificationEmail email)
        {
            this.Email = email;
        }
    }
}
