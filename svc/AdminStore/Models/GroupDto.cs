using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminStore.Models.Enums;

namespace AdminStore.Models
{
    public class GroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Scope { get; set; }
        public string LicenseType { get; set; }
        public string Source { get; set; }
        public string Email { get; set; }
        public string GroupType { get; set; }
    }
}