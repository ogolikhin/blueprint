﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Models
{
    public class CreateRoleAssignment
    {
        public int GroupId { get; set; }
        public int RoleId { get; set; }
    }
}