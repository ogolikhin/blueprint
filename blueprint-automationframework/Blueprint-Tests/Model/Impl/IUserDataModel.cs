using System.Collections.Generic;
using Newtonsoft.Json;
using Utilities;

namespace Model.Impl
{
    public interface IUserDataModel
    {
        #region Properties

        [JsonProperty("Type")]
        string UserOrGroupType { get; set; }

        int? Id { get; set; }

        [JsonProperty("Name")]
        string Username { get; set; }

        string DisplayName { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }

        [JsonConverter(typeof(SerializationUtilities.ConcreteListConverter<IGroup, Group>))]
        List<IGroup> Groups { get; set; }
        List<int> GroupIds { get; set; }

        string Title { get; set; }
        string Department { get; set; }
        string Password { get; set; }
        bool? ExpiredPassword { get; set; }
        string InstanceAdminRole { get; set; }
        bool? Enabled { get; set; }
        bool? FallBack { set; get; }
        string Email { get; set; }

        #endregion Properties
    }
}
