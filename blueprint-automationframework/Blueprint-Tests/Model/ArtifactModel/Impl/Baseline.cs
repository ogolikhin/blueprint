using NUnit.Framework;
using System.Collections.Generic;
using Utilities;

namespace Model.ArtifactModel.Impl
{
    public class Baseline : NovaCollectionBase
    {
        #region JSON Properties
        public bool IsAvailableInAnalytics { get; set; }

        public bool NotAllArtifactsAreShown { get; set; }

        public bool IsSealed { get; set; }

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

        /// <summary>
        /// Updates Collection's artifacts.
        /// </summary>
        /// <param name="artifactsIdsToAdd">List of artifact's Id to add to Collection.</param>
        /// <param name="artifactsIdsToRemove">List of artifact's Id to remove from Collection.</param>
        public void UpdateArtifacts(List<int> artifactsIdsToAdd = null, List<int> artifactsIdsToRemove = null)
        {
            UpdateArtifacts(PropertyTypePredefined.BaselineContent, artifactsIdsToAdd, artifactsIdsToRemove);
        }

        /// <summary>
        /// Compare two Baselines
        /// </summary>
        /// <param name="expectedBaseline">Expected Baseline</param>
        /// <param name="actualBaseline">Actual Baseline</param>
        /// <param name="skipArtifacts">(optional) Pass false to compare Artifacts</param>
        /// <param name="skipNovaArtifactDetails">(optional) Pass false to compare Baseline's properties common for all artifacts</param>
        public static void AssertBaselinesAreEqual(NovaCollectionBase expectedBaseline, NovaCollectionBase actualBaseline,
            bool skipArtifacts = true)
        {
            ThrowIf.ArgumentNull(expectedBaseline, nameof(expectedBaseline));
            ThrowIf.ArgumentNull(actualBaseline, nameof(actualBaseline));

            ObjectsCompare.CompareBasicTypeFields(expectedBaseline, actualBaseline);

            if (!skipArtifacts)
            {
                Assert.AreEqual(expectedBaseline.Artifacts?.Count, actualBaseline.Artifacts?.Count,
                    "Baseline should have expected number of Artifacts.");
                // TODO: add comparison of Artifacts
            }
        }
    }
}
