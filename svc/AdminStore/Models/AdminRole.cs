using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminStore.Models
{
    public class AdminRole
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Privileges { get; set; }
    }
}