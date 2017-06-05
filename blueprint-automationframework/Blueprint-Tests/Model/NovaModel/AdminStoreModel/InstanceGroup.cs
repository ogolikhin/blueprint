using System.Collections.Generic;
using Model.Common.Enums;

namespace Model.NovaModel.AdminStoreModel
{
    // Similar to GroupDto found in: blueprint/svc/AdminStore/Models/GroupDto.cs (in bp-offshore/blueprint repo)
    public class InstanceGroup
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Scope { get; set; }

        public LicenseLevel LicenseType { get; set; }

        public UserGroupSource GroupSource { get; set; }

        public string Email { get; set; }

        public int? ProjectId { get; set; }

        public List<int> Users { get; set; }

        public int CurrentVersion { get; set; }

        public string GroupType { get; set; }
    }
}
