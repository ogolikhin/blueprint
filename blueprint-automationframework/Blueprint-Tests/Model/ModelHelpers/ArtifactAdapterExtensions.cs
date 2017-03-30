using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Impl;
using NUnit.Framework;
using Utilities;

namespace Model.ModelHelpers
{
    public static class ArtifactAdapterExtensions
    {
        // XXX: This is an experimental adapter.  Do not check in yet!
        public static IArtifact ConvertToIArtifact(this INovaArtifactDetails novaArtifact)
        {
            ThrowIf.ArgumentNull(novaArtifact, nameof(novaArtifact));
            Assert.NotNull(novaArtifact.ItemTypeId, "'{0}' shouldn't be null!", nameof(novaArtifact.ItemTypeId));
            Assert.NotNull(novaArtifact.ParentId, "'{0}' shouldn't be null!", nameof(novaArtifact.ParentId));
            Assert.NotNull(novaArtifact.ProjectId, "'{0}' shouldn't be null!", nameof(novaArtifact.ProjectId));
            Assert.NotNull(novaArtifact.Version, "'{0}' shouldn't be null!", nameof(novaArtifact.Version));

            return new Artifact
            {
                Address = null,
//                AreAttachmentsReadOnly = null,
//                AreDocumentReferencesReadOnly = null,
//                AreTracesReadOnly = null,
                ArtifactTypeId = novaArtifact.ItemTypeId.Value,
                ArtifactTypeName = novaArtifact.ItemTypeName,
                BaseArtifactType = (BaseArtifactType)novaArtifact.ItemTypeId.Value,
                BlueprintUrl = null,
                CreatedBy = new DatabaseUser { Id = novaArtifact.CreatedBy.Id, DisplayName = novaArtifact.CreatedBy.DisplayName },
                Id = novaArtifact.Id,
                Name = novaArtifact.Name,
                ParentId = novaArtifact.ParentId.Value,
                ProjectId = novaArtifact.ProjectId.Value,
                Version = novaArtifact.Version.Value,

//                Properties = ???
            };
        }
    }
}
