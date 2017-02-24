using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Model.Impl
{
    public class UserDataModel
    {
        #region Properties

        [JsonProperty("Type")]
        public string UserOrGroupType { get; set; }

        public int? Id { get; set; }

        [JsonProperty("Name")]
        public string Username { get; set; }

        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public List<Group> Groups { get; set; } = new List<Group>();
        public List<int> GroupIds { get; set; }

        public string Title { get; set; }
        public string Department { get; set; }
        public string Password { get; set; }
        public bool? ExpirePassword { get; set; }
        public string InstanceAdminRole { get; set; }
        public bool? Enabled { get; set; }
        public bool? FallBack { set; get; }
        public string Email { get; set; }

        // Don't serialize Groups property if empty list.
        public virtual bool ShouldSerializeGroups()
        {
            return Groups.Count > 0;
        }

        #endregion Properties
    }
}
