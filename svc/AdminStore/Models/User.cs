using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace AdminStore.Models
{
    [JsonObject]
    public class User : LoginUser
    {
        /// <summary>
        /// Defines whether or not the user can login to the system.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// For database users, defines whether their password will never expire.
        /// </summary>
        public bool ExpirePassword { get; set; }


        /// <summary>
        /// ImageId of added user’s picture. 
        /// </summary>
        public int? ImageId { get; set; }


        /// <summary>
        /// Defines the password of the user.
        /// </summary>
        public string Password { get; set; }


        /// <summary>
        /// Database saves HASH of password and SALT(GUID).
        /// </summary>
        [JsonIgnore]
        public Guid? UserSALT { get; set; }


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
        public int [] GroupMembership { get; set; }


        /// <summary>
        ///  A guest user is someone who has been mentioned by email address in a Blueprint discussion. 
        /// </summary>
        public bool Guest { get; set; }


    }
}