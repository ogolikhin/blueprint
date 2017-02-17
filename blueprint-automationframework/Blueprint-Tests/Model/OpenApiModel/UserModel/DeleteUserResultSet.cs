using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace Model.OpenApiModel.UserModel
{
    /// <summary>
    /// Describes response returned from DeleteUser call
    /// </summary>
    public class DeleteUserResultSet
    {
        public List<DeleteUserResult> Results { get; set; }
        public HttpStatusCode ReturnCode { get; set; }
    }

    public class DeleteUserResult
    {
        public DeleteUserInfo User { get; set; }
        public string Message { get; set; }
        public int ResultCode { get; set; }
    }

    public class DeleteUserInfo
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

        public string Email { get; set; }

        #endregion Serialized properties
    }
}
