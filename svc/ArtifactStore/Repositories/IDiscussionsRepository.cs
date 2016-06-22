using ArtifactStore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    public interface IDiscussionsRepository
    {
        Task<IEnumerable<Discussion>> GetDiscussions(int itemId, int projectId);

        Task<IEnumerable<Reply>> GetReplies(int discussionId, int projectId);
    }
}