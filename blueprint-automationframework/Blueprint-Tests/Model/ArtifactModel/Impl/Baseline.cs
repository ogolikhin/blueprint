using System.Collections.Generic;

namespace Model.ArtifactModel.Impl
{
    public class Baseline : NovaArtifactDetails
    {
        #region JSON Properties
        public bool IsAvailableInAnalytics { get; set; }

        public bool NotAllArtifactsAreShown { get; set; }

        public bool IsSealed { get; set; }

        public List<CollectionItem> Artifacts { get; set; }
        #endregion

        #region Constructors
        public Baseline(bool isAvailableInAnalytics, bool notAllArtifactsAreShown, bool isSealed)
        {
            IsAvailableInAnalytics = isAvailableInAnalytics;
            NotAllArtifactsAreShown = notAllArtifactsAreShown;
            IsSealed = isSealed;
        }

        public Baseline() //parameterless constructor is required to deserialize JSON
        { }
        #endregion
    }
}
