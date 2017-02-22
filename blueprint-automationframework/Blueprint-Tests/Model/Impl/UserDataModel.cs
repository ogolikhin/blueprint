using Newtonsoft.Json;
using System.Collections.Generic;

namespace Model.Impl
{
    public class UserDataModel
    {
        #region Properties

        public int Id { get; set; }

        [JsonProperty("Name")]
        public string Username { get; set; }

        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public string Password { get; set; }
        public List<IGroup> Groups { get; set; } = new List<IGroup>();
        public List<int> GroupIds { get; set; }
        public string InstanceAdminRole { get; set; }
        public bool? ExpirePassword { get; set; }
        public bool Enabled { get; set; }

        [JsonProperty("Type")]
        public string UserOrGroupType { get; set; }

        #endregion Properties
    }
}
