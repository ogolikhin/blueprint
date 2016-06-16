using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArtifactStore.Models;
using ServiceLibrary.Models;

namespace ArtifactStore.Repositories
{

    public class SqlDiscussionsRepository : IDiscussionsRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;

        internal readonly IUsersRepository SqlUsersRepository;

        public SqlDiscussionsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain), new SqlUsersRepository())
        {
        }

        internal SqlDiscussionsRepository(ISqlConnectionWrapper connectionWrapper, IUsersRepository sqlUsersRepository)
        {
            ConnectionWrapper = connectionWrapper;
            SqlUsersRepository = sqlUsersRepository;
        }

        public async Task<IEnumerable<Discussion>> GetDiscussions(int itemId)
        {
            var discussionsPrm = new DynamicParameters();
            discussionsPrm.Add("@itemId", itemId);

            var discussions = (await ConnectionWrapper.QueryAsync<Discussion>("NOVA_GetItemDiscussions", discussionsPrm, commandType: CommandType.StoredProcedure)).ToList();
            await InitializeDiscussionProperties(discussions);

            return discussions;
        }

        public async Task<IEnumerable<Reply>> GetReplies(int discussionId)
        {
            var repliesPrm = new DynamicParameters();
            repliesPrm.Add("@discussionId", discussionId);

            var replies = (await ConnectionWrapper.QueryAsync<Reply>("NOVA_GetItemReplies", repliesPrm, commandType: CommandType.StoredProcedure)).ToList();
            await InitializeDiscussionProperties(replies);

            return replies;
        }

        private async Task InitializeDiscussionProperties(IEnumerable<CommentBase> comments)
        {
            if (!comments.Any())
            {
                return;
            }
            var userIds = new HashSet<int>(comments.Select(d => d.UserId));
            var userInfos = (await SqlUsersRepository.GetUserInfos(userIds)).ToDictionary(u => u.UserId);
            foreach (var comment in comments)
            {
                var userInfo = (UserInfo)null;
                if (userInfos.TryGetValue(comment.UserId, out userInfo))
                {
                    comment.UserName = userInfo.DisplayName;
                    comment.IsGuest = userInfo.IsGuest;
                }
            }
        }
    }
}