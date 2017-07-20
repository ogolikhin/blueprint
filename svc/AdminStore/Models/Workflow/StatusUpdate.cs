using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminStore.Models.Workflow
{
    public class StatusUpdate
    {
        public int VersionId { get; set; }
        public bool Status { get; set; }
    }
}