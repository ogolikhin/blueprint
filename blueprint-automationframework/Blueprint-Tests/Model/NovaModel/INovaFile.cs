using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Model.NovaModel
{
    public interface INovaFileMetadata
    {

        #region JSON serialzied properties
        string Guid { get; set; }
        Uri UriToFile { get; set; }
        #endregion JSON serialized properties

        [JsonIgnore]
        string FileName { get; set; }
        [JsonIgnore]
        string FileType { get; set; }
        [JsonIgnore]
        DateTime LastModifiedDate { get; set; }
    }


    public interface INovaFile : INovaFileMetadata
    {
        /// <summary>
        /// The file data.
        /// </summary>
        [JsonIgnore]
        IEnumerable<byte> Content { get; set; }
        [JsonIgnore]
        long ContentLength { get; set; }
    }
}
