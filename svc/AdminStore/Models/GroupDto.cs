﻿using System;
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
    }
}