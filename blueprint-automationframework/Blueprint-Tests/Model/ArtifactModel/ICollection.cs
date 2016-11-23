using System.Collections.Generic;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;

namespace Model
{
    public interface ICollection //: INovaArtifactDetails
    {
        string ReviewName { get; set; }
        bool IsCreated { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        List<CollectionItem> Artifacts { get; set; }
        void UpdateArtifacts(List<int> artifactsIdsToAdd = null, List<int> artifactsIdsToRemove = null);
    }
}
