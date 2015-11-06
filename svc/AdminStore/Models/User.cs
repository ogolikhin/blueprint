using System;

namespace AdminStore.Models
{
    public class User
    {
        public int Id { get; set; }

        public bool IsEnabled { get; set; }

        public string Password { get; set; }

        public bool? ExpirePassword { get; set; }

        public Guid UserSalt { get; set; }

        public UserGroupSource Source { get; set; }

        public DateTime? LastPasswordChangeTimestamp { get; set; }
    }
}