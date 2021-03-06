﻿using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace AdminStore.Models.DTO
{
    [JsonObject]
    public class User
    {
        /// <summary>
        /// The user's login.
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// The authentication source of the user.
        /// </summary>
        public UserGroupSource Source { get; set; }

        /// <summary>
        /// The Instance Admin Role ID of this user, if any.
        /// </summary>
        public int? InstanceAdminRoleId { get; set; }

        /// <summary>
        /// True if this user is allowed to fallback to non-SSO authentication.
        /// </summary>
        public bool? AllowFallback { get; set; }

        /// <summary>
        /// Defines whether or not the user can login to the system.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// For database users, defines whether their password will never expire.
        /// </summary>
        public bool ExpirePassword { get; set; }

        /// <summary>
        /// The name to display for this user.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The user's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The user's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// ImageId of added user’s picture.
        /// </summary>
        public int? ImageId { get; set; }

        /// <summary>
        /// Defines the password of the user.
        /// </summary>
        public string NewPassword { get; set; }

        /// <summary>
        /// The user's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Defines the job title of the user.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Defines the department to which the user belongs.
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// Defines the groups to which the user is a member.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public int[] GroupMembership { get; set; }

        /// <summary>
        ///  A guest user is someone who has been mentioned by email address in a Blueprint discussion.
        /// </summary>
        public bool Guest { get; set; }
    }
}