using NUnit.Framework;
using System.Collections.Generic;
using Model.Common.Enums;
using Utilities;

namespace Model.ArtifactModel.Impl
{
    public class Baseline : NovaCollectionBase
    {
        #region JSON Properties
        public bool IsAvailableInAnalytics {
            get { return isAvailableInAnalytics; }
            set { isAvailableInAnalytics = value; }
        }

        private bool isAvailableInAnalytics;

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
        /// Updates Baseline's artifacts.
        /// </summary>
        /// <param name="artifactsIdsToAdd">(optional) List of artifact's Id to add to Baseline.</param>
        /// <param name="artifactsIdsToRemove">(optional) List of artifact's Id to remove from Baseline.</param>
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

        /// <summary>
        /// Sets IsAvailableInAnalytics flag in SpecificPropertyValues to make it available for ArtifactUpdate
        /// </summary>
        /// <param name="availableInAnalytics">value to set</param>
        public void SetIsAvailableInAnalytics(bool availableInAnalytics)
        {
            var specProperty = SpecificPropertyValues.Find(property => property.PropertyType == PropertyTypePredefined.BaselineIsDataAnalyticsAvailable);
            if (specProperty != null)
            {
                specProperty.CustomPropertyValue = availableInAnalytics;
            }
            else
            {
                var analyticsProperty = new CustomProperty();
                analyticsProperty.Name = nameof(PropertyTypePredefined.BaselineIsDataAnalyticsAvailable);
                analyticsProperty.PropertyTypeId = -1;
                analyticsProperty.PropertyType = PropertyTypePredefined.BaselineIsDataAnalyticsAvailable;
                analyticsProperty.CustomPropertyValue = availableInAnalytics;
                
                SpecificPropertyValues.Add(analyticsProperty);
            }

            isAvailableInAnalytics = availableInAnalytics;
        }
    }
}
