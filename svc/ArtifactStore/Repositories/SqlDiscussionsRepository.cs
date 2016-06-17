using ArtifactStore.Helpers;
using ArtifactStore.Models;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{

    public class SqlDiscussionsRepository : IDiscussionsRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;

        internal readonly IUsersRepository SqlUsersRepository;

        private readonly MentionHelper _mentionHelper;

        public SqlDiscussionsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain), new SqlUsersRepository())
        {
        }

        internal SqlDiscussionsRepository(ISqlConnectionWrapper connectionWrapper, IUsersRepository sqlUsersRepository)
        {
            ConnectionWrapper = connectionWrapper;
            SqlUsersRepository = sqlUsersRepository;
            _mentionHelper = new MentionHelper(sqlUsersRepository);
        }

        public async Task<IEnumerable<Discussion>> GetDiscussions(int itemId)
        {
            var discussionsPrm = new DynamicParameters();
            discussionsPrm.Add("@itemId", itemId);

            var discussions = (await ConnectionWrapper.QueryAsync<Discussion>("GetItemDiscussions", discussionsPrm, commandType: CommandType.StoredProcedure)).ToList();
            await InitializeDiscussionProperties(discussions);

            return discussions;
        }

        public async Task<IEnumerable<Reply>> GetReplies(int discussionId)
        {
            var repliesPrm = new DynamicParameters();
            repliesPrm.Add("@discussionId", discussionId);

            var replies = (await ConnectionWrapper.QueryAsync<Reply>("GetItemReplies", repliesPrm, commandType: CommandType.StoredProcedure)).ToList();
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
                    comment.Comment = _mentionHelper.ProcessComment(comment.Comment, comment.ItemId);
                }
            }
        }
    }
}