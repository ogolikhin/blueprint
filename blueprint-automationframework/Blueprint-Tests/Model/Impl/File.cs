using System;
using System.Collections.Generic;

namespace Model.Impl
{
    public class File : IFile
    {
        #region Implements IFile

        public IEnumerable<byte> Content { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string Guid { get; set; }
        public DateTime LastModifiedDate { get; set; }

        #endregion Implements IFile
    }

    /// <summary>
    /// This class represents an embedded image with data from the EmbeddedImages table of the Blueprint (Raptor) database.
    /// </summary>
    public class EmbeddedImageFile : File
    {
        public int? ArtifactId { get; set; }
        public string EmbeddedImageId { get; set; }
        public DateTime? ExpireTime { get; set; }
    }
}
