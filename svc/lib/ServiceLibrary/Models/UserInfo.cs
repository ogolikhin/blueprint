namespace ServiceLibrary.Models
{
    public class UserInfo
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; }
        public int? ImageId { get; set; }
        public bool IsGuest { get; set; }
        public bool IsEnabled { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
    }
}
