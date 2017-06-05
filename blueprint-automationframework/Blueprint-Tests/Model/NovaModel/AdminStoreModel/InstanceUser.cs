using System.Diagnostics.CodeAnalysis;
using Model.Common.Enums;
using Model.Impl;
using Newtonsoft.Json;

namespace Model.NovaModel.AdminStoreModel
{
    //class for object returned by adminstore/users
    public class InstanceUser : LoginUser
    {
        /// <summary>
        /// True if the user is a guest user, false otherwise.
        /// </summary>
        public bool Guest { get; set; }

        /// <summary>
        /// The current version of the user.
        /// </summary>
        public int CurrentVersion { get; set; }

        /// <summary>
        /// True if the user is enabled, false otherwise.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The user's job title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The user's department.
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// True if the user's password can expire, false otherwise.
        /// </summary>
        public bool? ExpirePassword { get; set; }

        /// <summary>
        /// The image id for the user
        /// </summary>
        [JsonProperty("Image_ImageId")]
        public int? ImageId { get; set; }

        /// <summary>
        /// A list of groups that the user belongs to
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public int[] GroupMembership { get; set; }

        /// <summary>
        /// A new password for the user.
        /// (Only to be used for create/update user)
        /// </summary>
        public string Password { get; set; }

        public InstanceUser(string login, string firstName, string lastName, string displayName, string email,
            UserGroupSource? source, bool eulaAccepted, LicenseLevel? license, bool isSso, bool? allowFallback, 
            InstanceAdminRole? instanceAdminRole, InstanceAdminPrivileges? instanceAdminPrivileges,
            bool guest, int currentVersion, bool enabled, string title, string department, bool? expirePassword, 
            int? imageId, int[] groupMembership, string password = null)
            : base(login, firstName, lastName, displayName, email, source, eulaAccepted, license, isSso, 
                  allowFallback, instanceAdminRole, instanceAdminPrivileges)
        {
            Guest = guest;
            CurrentVersion = currentVersion;
            Enabled = enabled;
            Title = title;
            Department = department;
            ExpirePassword = expirePassword;
            ImageId = imageId;
            GroupMembership = groupMembership;
            Password = password;
        }
    }
}
