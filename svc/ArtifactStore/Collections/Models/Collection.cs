using System;

namespace ArtifactStore.Collections.Models
{
    public class Collection
    {
        public int Id { get; }

        public int ProjectId { get; }

        public int? LockedByUserId { get; }

        public Collection(int id, int projectId, int? lockedByUserId)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            if (projectId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(projectId));
            }

            Id = id;
            ProjectId = projectId;
            LockedByUserId = lockedByUserId;
        }
    }
}
