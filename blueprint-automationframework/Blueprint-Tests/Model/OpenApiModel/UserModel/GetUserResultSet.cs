using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Model.OpenApiModel.UserModel
{
    public class GetUserResultSet
    {
        public List<GetUserResult> Users { get; set; }
    }

    // Dev code can be found in:  blueprint-current/Source/BluePrintSys.RC.Api.Business/Models/User.cs
    public class GetUserResult
    {
        #region Serialized properties

        [JsonProperty("Type")]
        public string UserOrGroupType { get; set; }
        public int Id { get; set; }

        [JsonProperty("Name")]
        public string Username { get; set; }
        public string DisplayName { get; set; }

        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public List<GetGroupResult> Groups { get; set; }

        public string Title { get; set; }
        public string Department { get; set; }
        public string Password { get; set; }
        public DateTime? PasswordExpired { get; set; }
        public string InstanceAdminRole { get; set; }
        public string Email { get; set; }

        #endregion Serialized properties
    }
}
