using Newtonsoft.Json;
using System;

namespace Model.NovaModel
{
    public interface INovaFileMetadata : IFileMetadata
    {
        #region JSON serialzied properties
        Uri UriToFile { get; set; }
        #endregion JSON serialized properties
    }


    public interface INovaFile : INovaFileMetadata, IFile
    {
        [JsonIgnore]
        long ContentLength { get; set; }
    }
}
