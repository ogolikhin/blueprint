using System;
using NUnit.Framework;
using System.Collections.Generic;
using Model.Common.Enums;
using Utilities;

namespace Model.ArtifactModel.Impl
{
    public class Baseline : NovaCollectionBase
    {
        #region JSON Properties

        public DateTime? MinimalUtcTimestamp { get; set; }

        public bool IsAvailableInAnalytics {
            get { return _isAvailableInAnalytics; }
            set { _isAvailableInAnalytics = value; }
        }

        private bool _isAvailableInAnalytics;

        public bool NotAllArtifactsAreShown { get; set; }

        public bool IsSealed {
            get { return _isSealed; }
            set { _isSealed = value; }
        }
        private bool _isSealed;

        // null for 'Live' baseline
        public DateTime? UtcTimestamp
        {
            get { return _utcTimestamp; }
            set { _utcTimestamp = value; }
        }
        private DateTime? _utcTimestamp;

        #endregion JSON Properties

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
        public void SetIsAvailableInAnalytics(bool availableInAnalytics) // TFS 5761
        {
            SetBaselineProperty(PropertyTypePredefined.BaselineIsDataAnalyticsAvailable, availableInAnalytics);
            _isAvailableInAnalytics = availableInAnalytics;
        }

        /// <summary>
        /// Sets UtcTimestamp flag in SpecificPropertyValues to make it available for ArtifactUpdate
        /// </summary>
        /// <param name="utcTimestamp">value to set</param>
        public void SetUtcTimestamp(DateTime utcTimestamp) // TFS 5761
        {
            SetBaselineProperty(PropertyTypePredefined.BaselineTimestamp, utcTimestamp);
            _utcTimestamp = utcTimestamp;
        }

        /// <summary>
        /// Sets IsSealed flag in SpecificPropertyValues to make it available for ArtifactUpdate
        /// </summary>
        /// <param name="isSealed">value to set</param>
        public void SetIsSealed(bool isSealed) // TFS 5761
        {
            SetBaselineProperty(PropertyTypePredefined.BaselineIsSealed, isSealed);
            _isSealed = isSealed;
        }

        /// <summary>
        /// Sets or adds property value to the specified value
        /// </summary>
        /// <param name="baselinePropertyType">Property type to set</param>
        /// <param name="baselinePropertyValue">Property type value to set</param>
        private void SetBaselineProperty(PropertyTypePredefined baselinePropertyType, object baselinePropertyValue) // TFS 5761
        {
            var specProperty = SpecificPropertyValues.Find(property => property.PropertyType == baselinePropertyType);
            if (specProperty != null)
            {
                specProperty.CustomPropertyValue = baselinePropertyValue;
            }
            else
            {
                specProperty = new CustomProperty();
                specProperty.Name = baselinePropertyType.ToString();
                specProperty.PropertyTypeId = -1;
                specProperty.PropertyType = baselinePropertyType;
                specProperty.CustomPropertyValue = baselinePropertyValue;
                
                SpecificPropertyValues.Add(specProperty);
            }
        }
    }

    public class AddToBaselineResult : AddToBaselineCollectionResult
    {
        public int? UnpublishedArtifactCount { get; set; }
        public int? NonExistentArtifactCount { get; set; }
    }
}
