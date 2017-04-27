using System;
using NUnit.Framework;
using System.Collections.Generic;
using Model.Common.Enums;
using Utilities;
using Newtonsoft.Json;

namespace Model.ArtifactModel.Impl
{
    public class Baseline : NovaCollectionBase
    {
        #region JSON Properties

        public DateTime? MinimalUtcTimestamp { get; set; }

        public bool NotAllArtifactsAreShown { get; set; }

        #endregion JSON Properties

        [JsonIgnore]
        public bool IsAvailableInAnalytics {
            get {
                return GetSpecificPropertyValue<bool>(PropertyTypePredefined.BaselineIsDataAnalyticsAvailable); }
            set { SetBaselineProperty(PropertyTypePredefined.BaselineIsDataAnalyticsAvailable, value); }
        }

        [JsonIgnore]
        public bool IsSealed {
            get { return GetSpecificPropertyValue<bool>(PropertyTypePredefined.BaselineIsSealed); }
            set { SetBaselineProperty(PropertyTypePredefined.BaselineIsSealed, value); }
        }

        // null for 'Live' baseline
        [JsonIgnore]
        public DateTime? UtcTimestamp
        {
            get { return GetSpecificPropertyValue<DateTime?>(PropertyTypePredefined.BaselineTimestamp); }
            set { SetBaselineProperty(PropertyTypePredefined.BaselineTimestamp, value); }
        }

        [JsonIgnore]
        public DateTime? BaselineCurrentClientUtcTime
        {
            get { return GetSpecificPropertyValue<DateTime?>(PropertyTypePredefined.BaselineCurrentClientUtcTime); }
            set { SetBaselineProperty(PropertyTypePredefined.BaselineCurrentClientUtcTime, value); }
        }

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

    public class BaselineInfo
    {
        #region JSON Properties
        public int ItemId { set; get; }
        public bool IsSealed { set; get; }
        public DateTime? UtcTimestamp { set; get; }
        #endregion

        public void AssertBaselineInfoCorrespondsToBaseline(Baseline baseline)
        {
            ThrowIf.ArgumentNull(baseline, nameof(baseline));
            Assert.AreEqual(ItemId, baseline.Id, "ItemId for BaselineInfo should be equal to Id for Baseline.");
            Assert.AreEqual(IsSealed, baseline.IsSealed, "IsSealed for BaselineInfo and for Baseline should be equal.");
            Assert.AreEqual(UtcTimestamp, baseline.UtcTimestamp, "UtcTimestamp for BaselineInfo and for Baseline should be equal.");
        }
    }
}
