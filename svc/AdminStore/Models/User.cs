﻿using System;
using System.Collections.Generic;

namespace AdminStore.Models
{
    public class User : LoginUser
    {
        public bool Guest { get; set; }
        public int CurrentVersion { get; set; }
        public string Password { get; set; }
        public bool Enabled { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public int InvalidLogonAttemptsNumber { get; set; }
        public DateTime? LastInvalidLogonTimeStamp { get; set; }
        public Guid UserSALT { get; set; }
        public bool? ExpirePassword { get; set; }
        public DateTime StartTimestamp { get; set; }
        public DateTime? EndTimestamp { get; set; }
        public DateTime? LastPasswordChangeTimestamp { get; set; }
        public int? Image_ImageId { get; set; }
        public string InstanceAdminRoleName { get; set; }
        public IEnumerable<int> GroupMembership { get; set; }
    }
}