using System.Collections.Generic;
using AdminStore.Models.Enums;

namespace AdminStore.Models
{
    public class GroupDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Scope { get; set; }

        public string LicenseType { get; set; }

        public LicenseType License { get; set; }

        public string Source { get; set; }

        public string Email { get; set; }

        public string GroupType { get; set; }

        public int? ProjectId { get; set; }

        public UserGroupSource GroupSource { get; set; }

        public IEnumerable<int> Users { get; set; }

        public int CurrentVersion { get; set; }
    }
}