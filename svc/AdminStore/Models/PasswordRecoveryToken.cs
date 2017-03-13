using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminStore.Models
{
    public class PasswordRecoveryToken
    {
        public string Login { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid RecoveryToken { get; set; }
    }
}