using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Impl
{
    public interface IPrimitiveFolder
    {
        [JsonProperty("Id")]
        string FolderId { get; }

        [JsonProperty("Name")]
        string FolderName { get; }

        [JsonProperty("Type")]
        string InstanceType { get; }
    }
}
