using System.Collections.Generic;
using System.Linq;
using Model.ArtifactModel;
using Utilities;

namespace Model.ModelHelpers
{
    public static class ArtifactObserverHelper
    {
        /// <summary>
        /// Notifies this observer about artifacts that were deleted and published.
        /// </summary>
        /// <param name="artifacts">The list of artifacts to update by removing all artifacts whose Ids appear in the list
        ///     of deleted artifact IDs.</param>
        /// <param name="deletedArtifactIds">The list of artifact IDs that were deleted.</param>
        public static void NotifyArtifactDeletion<T>(List<T> artifacts, IEnumerable<int> deletedArtifactIds) where T : IArtifactBase
        {
            ThrowIf.ArgumentNull(artifacts, nameof(artifacts));
            ThrowIf.ArgumentNull(deletedArtifactIds, nameof(deletedArtifactIds));

            var artifactIds = deletedArtifactIds as IList<int> ?? deletedArtifactIds.ToList();
            var artifactsToRemove = new List<int>();

            foreach (var deletedArtifactId in artifactIds)
            {
                artifacts.ForEach(a =>
                {
                    if (a.Id == deletedArtifactId)
                    {
                        a.IsDeleted = true;
                        a.IsPublished = false;
                        a.IsSaved = false;

                        artifactsToRemove.Add(a.Id);
                    }
                });

                artifacts.RemoveAll(a => artifactsToRemove.Contains(a.Id));
            }
        }

        /// <summary>
        /// Notifies this observer about artifacts that were discarded.  If the artifact was never published, it is removed from the artifact list.
        /// </summary>
        /// <param name="artifacts">The list of artifacts to update by removing all unpublished artifacts whose Ids appear in the list
        ///     of discarded artifact IDs.</param>
        /// <param name="discardedArtifactIds">The list of artifact IDs that were discarded.</param>
        public static void NotifyArtifactDiscarded<T>(List<T> artifacts, IEnumerable<int> discardedArtifactIds) where T : IArtifactBase
        {
            ThrowIf.ArgumentNull(artifacts, nameof(artifacts));
            ThrowIf.ArgumentNull(discardedArtifactIds, nameof(discardedArtifactIds));

            var artifactIds = discardedArtifactIds as int[] ?? discardedArtifactIds.ToArray();
            var artifactsToRemove = new List<int>();

            foreach (var discardedArtifactId in artifactIds)
            {
                artifacts.ForEach(a =>
                {
                    if ((a.Id == discardedArtifactId) && !a.IsPublished)
                    {
                        a.IsDeleted = true;
                        a.IsPublished = false;
                        a.IsSaved = false;
                        a.Status.IsLocked = false;

                        artifactsToRemove.Add(a.Id);
                    }
                });

                artifacts.RemoveAll(a => artifactsToRemove.Contains(a.Id));
            }
        }

        /// <summary>
        /// Notifies this observer about artifacts that were published.
        /// </summary>
        /// <param name="artifacts">The list of artifacts to update by removing all artifacts that are marked for deletion whose Ids
        ///     appear in the list of published artifact IDs.</param>
        /// <param name="publishedArtifactIds">The list of artifact IDs that were published.</param>
        public static void NotifyArtifactPublish<T>(List<T> artifacts, IEnumerable<int> publishedArtifactIds) where T : IArtifactBase
        {
            ThrowIf.ArgumentNull(artifacts, nameof(artifacts));
            ThrowIf.ArgumentNull(publishedArtifactIds, nameof(publishedArtifactIds));

            var artifactIds = publishedArtifactIds as IList<int> ?? publishedArtifactIds.ToList();
            var artifactsToRemove = new List<int>();

            foreach (var publishedArtifactId in artifactIds)
            {
                artifacts.ForEach(a =>
                {
                    a.LockOwner = null;

                    if (a.Id == publishedArtifactId)
                    {
                        if (a.IsMarkedForDeletion)
                        {
                            a.IsDeleted = true;
                            a.IsPublished = false;

                            artifactsToRemove.Add(a.Id);
                        }
                        else
                        {
                            a.IsPublished = true;
                        }

                        a.IsSaved = false;
                    }
                });

                artifacts.RemoveAll(a => artifactsToRemove.Contains(a.Id));
            }
        }
    }
}
