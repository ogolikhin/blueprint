using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SearchService.Models
{
    public class ItemSearchResultItem
    {
        public int ProjectId { get; set; }

        public int ArtifactId { get; set; }

        public int ItemId { get; set; }

        public string Name { get; set; }
       
        public int ItemTypeId { get; set; }

        public string TypeName { get; set; }

        public string TypePrefix { get; set; }

        public string ArtifactPath { get; set; }
    }
}