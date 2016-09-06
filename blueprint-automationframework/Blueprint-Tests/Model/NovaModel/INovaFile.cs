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

        string FileName { get; set; }
        string FileType { get; set; }
        DateTime LastModifiedDate { get; set; }
    }


    public interface INovaFile : INovaFileMetadata
    {
        /// <summary>
        /// The file data.
        /// </summary>
        IEnumerable<byte> Content { get; set; }
        long ContentLength { get; set; }
    }
}
