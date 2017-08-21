using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminStore.Services.Email
{
    public class EmailClientConfig
    {
        public EmailClientType ClientType { get; set; }
        public string ServerAddress { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string AccountUsername { get; set; }
        public string AccountPassword { get; set; }
    }
}
