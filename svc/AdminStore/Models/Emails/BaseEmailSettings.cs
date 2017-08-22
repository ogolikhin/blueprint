using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminStore.Models.Emails
{
    public class BaseEmailSettings
    {
        public string ServerAddress { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string AccountUsername { get; set; }
        public string AccountPassword { get; set; }
    }
}
