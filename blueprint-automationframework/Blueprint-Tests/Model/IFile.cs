using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Model
{
    public interface IFileMetadata
    {
        #region JSON serialzied properties
        string Guid { get; set; }
        #endregion JSON serialized properties

        [JsonIgnore]
        string FileName { get; set; }
        [JsonIgnore]
        string FileType { get; set; }
        [JsonIgnore]
        DateTime LastModifiedDate { get; set; }
    }


    public interface IFile : IFileMetadata
    {
        /// <summary>
        /// The file data.
        /// </summary>
        [JsonIgnore]
        IEnumerable<byte> Content { get; set; }
    }
}
