using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminStore.Models
{
    public class RolesAssignments
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int GroupId { get; set; }
    }
}