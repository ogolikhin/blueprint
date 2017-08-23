using System.Collections.Generic;
using AdminStore.Models.Enums;

namespace AdminStore.Models
{
    public class GroupDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Scope { get; set; }

        public LicenseType LicenseType { get; set; }

        //public string LicenseTypeString
        //{
        //    get { return LicenseType.ToString(); }
        //}

        public UserGroupSource Source { get; set; }

        public string Email { get; set; }

        public int? ProjectId { get; set; }

        public int CurrentVersion { get; set; }
    }
}