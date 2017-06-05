using Model.ArtifactModel.Impl;
using Model.Common.Enums;
using Newtonsoft.Json;

namespace Model.NovaModel.AdminStoreModel
{
    // Similar to GroupUser found in: blueprint/svc/AdminStore/Models/GroupUser.cs (in bp-offshore/blueprint repo)
    public class InstanceGroupUser
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        [JsonProperty ("Type")]
        public UsersAndGroupsType UserGroupType { get; set; }
        public string Scope { get; set; }
        public LicenseLevel LicenseType { get; set; }
        public UserGroupSource GroupSource { get; set; }
    }
}
