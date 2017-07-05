using System;
using System.Collections.Generic;
using System.Linq;
using ArtifactStore.Models.Reuse;
using ArtifactStore.Repositories.VersionControl;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.VersionControl;

namespace ArtifactStore.Models.VersionControl
{
    public class PublishEnvironment
    {
        public int RevisionId { get; set; }

        public DateTime Timestamp { get; set; }

        private ISet<int> AffectedArtifactIds { get; set; }

        public void AddAffectedArtifact(int artifactId)
        {
            if (AffectedArtifactIds == null)
            {
                AffectedArtifactIds = new HashSet<int>();
            }

            if (!IsArtifactDeleted(artifactId))
            {
                AffectedArtifactIds.Add(artifactId);
            }
        }

        public ISet<int> GetAffectedArtifacts()
        {
            if (AffectedArtifactIds == null)
            {
                AffectedArtifactIds = new HashSet<int>();
            }
            return AffectedArtifactIds;
        }

        public bool IsArtifactDeleted(int artifactId)
        {
            return DeletedArtifactIds?.Contains(artifactId) ?? false;
        }

        public ISet<int> DeletedArtifactIds { get; } = new HashSet<int>();
        public bool KeepLock { get; set; }

        public IDictionary<int, SqlItemInfo> ArtifactStates { get; internal set; }

        public IEnumerable<int> FilterByBaseType(IEnumerable<int> artifactIds, ItemTypePredefined baseType)
        {
            foreach (var artifactId in artifactIds)
            {
                SqlItemInfo itemInfo;
                ArtifactStates.TryGetValue(artifactId, out itemInfo);

                if (itemInfo == null || itemInfo.PrimitiveItemTypePredefined != baseType)
                {
                    continue;
                }

                yield return artifactId;
            }
        }

        public ItemTypePredefined GetArtifactBaseType(int artifactId)
        {
            SqlItemInfo itemInfo;
            ArtifactStates.TryGetValue(artifactId, out itemInfo);
            if (itemInfo == null)
            {
                return ItemTypePredefined.None;
            }

            return itemInfo.PrimitiveItemTypePredefined;
        }

        public IDictionary<int, SqlPublishResult> PublishResults { get; private set; }
        public IEnumerable<IPublishRepository> Repositories { get; internal set; }
        public ReuseSensitivityCollector SensitivityCollector { get; internal set; }

        public SqlPublishResult GetPublishResult(int artifactId)
        {
            if (PublishResults == null)
            {
                PublishResults = new Dictionary<int, SqlPublishResult>();
            }

            SqlPublishResult result;
            if (!PublishResults.TryGetValue(artifactId, out result))
            {
                result = new SqlPublishResult(artifactId);
                PublishResults.Add(artifactId, result);
            }

            return result;
        }

        public IEnumerable<SqlPublishResult> GetChangeSqlPublishResults()
        {
            if (PublishResults == null)
                return Enumerable.Empty<SqlPublishResult>();

            return PublishResults
                .Where(pair => pair.Value.Changed)
                .Select(pair => pair.Value);
        }

        internal IEnumerable<int> GetArtifactsMovedAcrossProjects(IEnumerable<int> artifactIds)
        {
            foreach (var artifactId in artifactIds)
            {
                SqlItemInfo itemInfo;
                ArtifactStates.TryGetValue(artifactId, out itemInfo);

                if (itemInfo == null || !itemInfo.MovedAcrossProjects)
                {
                    continue;
                }

                yield return artifactId;
            }
        }
    }

    

    

    

    //public class UserValue : IEquatable<UserValue>
    //{
    //    private readonly IEnumerable<string> _userLabels;
    //    private readonly IEnumerable<KeyValuePair<int, UserType>> _userValues;

    //    public UserValue(IEnumerable<string> userLabels, IEnumerable<KeyValuePair<int, UserType>> userValues)
    //    {
    //        _userLabels = userLabels;
    //        _userValues = userValues;
    //    }

    //    public bool Equals(UserValue other)
    //    {
    //        if (other == null)
    //            return false;

    //        if (_userLabels == null)
    //        {
    //            if (other._userLabels != null)
    //                return false;

    //        }
    //        else
    //        {
    //            if (other._userLabels == null)
    //                return false;

    //            return !_userLabels.Except(other._userLabels).Any() && !other._userLabels.Except(_userLabels).Any();
    //        }

    //        if (_userValues == null)
    //        {
    //            if (other._userValues == null || !other._userValues.Any())
    //                return true;
    //            return false;
    //        }

    //        if (other._userValues == null)
    //        {
    //            return !_userValues.Any();
    //        }

    //        return !_userValues.Except(other._userValues).Any() && !other._userValues.Except(_userValues).Any();
    //    }
    //}
}