using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Impl
{
    public class PrimitiveFolder : IPrimitiveFolder
    {
        [JsonProperty("Id")]
        public string FolderId { get; set; }

        [JsonProperty("ParentFolderId")]
        public string ParentFolderId { get; set; }

        [JsonProperty("Name")]
        public string FolderName { get; set; }

        [JsonProperty("Type")]
        public int InstanceType { get; set; }
    }
}
