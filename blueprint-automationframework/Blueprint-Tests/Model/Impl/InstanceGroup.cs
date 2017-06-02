using System.Collections.Generic;
using Model.Common.Enums;

namespace Model.Impl
{
    public class InstanceGroup
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Scope { get; set; }

        public LicenseLevel LicenseType { get; set; }

        public UserGroupSource GroupSource { get; set; }

        public string Email { get; set; }

        public int? ProjectId { get; set; }

        public IEnumerable<int> Users { get; set; }

        public int CurrentVersion { get; set; }

        public string GroupType { get; set; }
    }
}
