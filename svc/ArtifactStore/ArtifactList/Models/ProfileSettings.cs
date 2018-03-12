using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtifactStore.ArtifactList.Models
{
    public class ProfileSettings
    {
        public ProfileColumns ProfileColumns { get; set; }
        public int? PaginationLimit { get; set; }
    }
}