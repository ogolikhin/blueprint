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

        public string FileName { get; set; }
        public string FileType { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public IEnumerable<byte> Content { get; set; }
        public long ContentLength { get; set; }
    }
}
