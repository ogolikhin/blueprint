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
        private string _password;

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

        // These properties were added during US4965.
        public string Title { get; set; }
        public string Department { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public string Password  // NOTE: Password should NEVER be returned by OpenAPI.
        {
            get
            {
                if (_password != null)
                {
                    throw new JsonException("GetUser should NEVER return a Password!");
                }
                return _password;
            }
            set { _password = value; }
        }
        public DateTime? PasswordExpired { get; set; }
        public string InstanceAdminRole { get; set; }
        public string Email { get; set; }

        #endregion Serialized properties
    }
}
