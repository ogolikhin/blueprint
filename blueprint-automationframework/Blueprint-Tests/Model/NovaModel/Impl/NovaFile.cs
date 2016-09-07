using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Model.NovaModel.Impl
{
    public class NovaFile : INovaFile
    {
        #region JSON serialzied properties
        public string Guid { get; set; }
        public Uri UriToFile { get; set; }
        #endregion JSON serialzied properties

        [JsonIgnore]
        public string FileName { get; set; }
        [JsonIgnore]
        public string FileType { get; set; }
        [JsonIgnore]
        public DateTime LastModifiedDate { get; set; }
        [JsonIgnore]
        public IEnumerable<byte> Content { get; set; }
        [JsonIgnore]
        public long ContentLength { get; set; }
    }
}
