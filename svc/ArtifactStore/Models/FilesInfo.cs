using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtifactStore.Models
{
    public class FilesInfo
    {
        private readonly IList<Attachment> _attachments;
        private readonly IList<DocumentReference> _documentReferences;

        public FilesInfo(IList<Attachment> attachments, IList<DocumentReference> documentReferences)
        {
            _attachments = attachments;
            _documentReferences = documentReferences;
        }

        public int ArtifactId { get; set; }

        public int? SubartifactId { get; set; }

        public IList<Attachment> Attachments
        {
            get
            {
                return _attachments;
            }
        }

        public IList<DocumentReference> DocumentReferences
        {
            get { return _documentReferences; }
        }
    }

    //public abstract class AbstractAttachment
    //{
    //    public int Id { get; set; }
    //    public string FileName { get; set; }
    //}

    public class Attachment
    {
        public string Name { get; set; }
        public Guid FileGuid { get; set; }
    }

    public class DocumentReference
    {
        public int VersionArtifactId { get; set; }
        public string Name { get; set; }     
    }
}