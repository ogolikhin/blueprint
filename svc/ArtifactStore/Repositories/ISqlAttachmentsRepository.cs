using ArtifactStore.Models;
using ServiceLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ArtifactStore.Repositories
{
    public interface ISqlAttachmentsRepository
    {
        Task<FilesInfo> GetAttachmentsAndDocumentReferences(int artifactId, int userId, int? subArtifactId = null,
            bool addDrafts = true);
    }
}