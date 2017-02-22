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
        /// Compare two Baselines
        /// </summary>
        /// <param name="expectedBaseline">Expected Baseline</param>
        /// <param name="actualBaseline">Actual Baseline</param>
        /// <param name="skipArtifacts">(optional) Pass false to compare Artifacts</param>
        /// <param name="skipNovaArtifactDetails">(optional) Pass false to compare Baseline's properties common for all artifacts</param>
        public static void AssertBaselinesAreEqual(Baseline expectedBaseline, Baseline actualBaseline,
            bool skipArtifacts = true, bool skipNovaArtifactDetails = true)
        {
            ThrowIf.ArgumentNull(expectedBaseline, nameof(expectedBaseline));
            ThrowIf.ArgumentNull(actualBaseline, nameof(actualBaseline));

            Assert.AreEqual(expectedBaseline.IsAvailableInAnalytics, actualBaseline.IsAvailableInAnalytics,
                "Baseline should have expected value of IsAvailableInAnalytics.");
            Assert.AreEqual(expectedBaseline.NotAllArtifactsAreShown, actualBaseline.NotAllArtifactsAreShown,
                "Baseline should have expected value of NotAllArtifactsAreShown.");
            Assert.AreEqual(expectedBaseline.IsSealed, actualBaseline.IsSealed,
                "Baseline should have expected value of IsSealed.");

            if (!skipArtifacts)
            {
                Assert.AreEqual(expectedBaseline.Artifacts?.Count, actualBaseline.Artifacts?.Count,
                    "Baseline should have expected number of Artifacts.");
                // TODO: add comparison of Artifacts
            }

            if (!skipNovaArtifactDetails)
            {
                AssertArtifactsEqual(expectedBaseline, actualBaseline);
            }
        }
    }
}
