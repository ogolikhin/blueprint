using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminStore.Models.Emails
{
    public class EmailSettingsDto
    {
        public EmailIncomingSettings Incoming { get; set; }
        public EmailOutgoingSettings Outgoing { get; set; }
    }
}
