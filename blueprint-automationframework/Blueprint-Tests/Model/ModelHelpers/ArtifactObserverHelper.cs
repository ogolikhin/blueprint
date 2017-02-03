using System.Collections.Generic;
using System.Linq;
using Model.ArtifactModel;
using Utilities;

namespace Model.ModelHelpers
{
    public static class ArtifactObserverHelper
    {
        /// <seealso cref="IArtifactObserver.NotifyArtifactDeletion(IEnumerable{int})" />
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

        /// <seealso cref="IArtifactObserver.NotifyArtifactDiscarded(IEnumerable{int})" />
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

        /// <seealso cref="IArtifactObserver.NotifyArtifactPublish(IEnumerable{int})" />
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
