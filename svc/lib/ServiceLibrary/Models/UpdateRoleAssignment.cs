using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Models
{
    public class UpdateRoleAssignment : CreateRoleAssignment
    {
        public int RoleAssignmentId { get; set; }
    }
}
