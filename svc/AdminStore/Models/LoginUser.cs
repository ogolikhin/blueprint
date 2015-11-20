﻿using System;
using System.Runtime.Serialization;

namespace AdminStore.Models
{
    public class LoginUser
    {
        public int Id { get; set; }

        public string Login { get; set; }

        [IgnoreDataMember]
        public string Password { get; set; }

        public bool IsEnabled { get; set; }

        public UserGroupSource Source { get; set; }

        public int InvalidLogonAttemptsNumber { get; set; }

        public DateTime? LastInvalidLogonTimeStamp { get; set; }

        public Guid UserSalt { get; set; }

        public bool? IsFallbackAllowed { get; set; }

        public DateTime? LastPasswordChangeTimestamp { get; set; }

        public bool? ExpirePassword { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public bool EULAccepted { get; set; }
    }
}