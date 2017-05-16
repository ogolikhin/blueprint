using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;

namespace AdminStore.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Scope { get; set; }
        public int LicenseId { get; set; }
        public byte Source { get; set; }    
    }
}