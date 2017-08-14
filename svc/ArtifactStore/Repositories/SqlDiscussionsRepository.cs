﻿using ArtifactStore.Helpers;
using ArtifactStore.Models;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Repositories.InstanceSettings;

namespace ArtifactStore.Repositories
{

    public class SqlDiscussionsRepository : IDiscussionsRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;
        private readonly IUsersRepository _sqlUsersRepository;
        private readonly MentionHelper _mentionHelper;

        public SqlDiscussionsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain),
                  new SqlUsersRepository(),
                  new SqlInstanceSettingsRepository(),
                  new SqlArtifactPermissionsRepository())
        {
        }

        internal SqlDiscussionsRepository(ISqlConnectionWrapper connectionWrapper,
            IUsersRepository sqlUsersRepository,
            IInstanceSettingsRepository instanceSettingsRepository,
            IArtifactPermissionsRepository permissionsRepository)
        {
            _connectionWrapper = connectionWrapper;
            _sqlUsersRepository = sqlUsersRepository;
            _mentionHelper = new MentionHelper(sqlUsersRepository, instanceSettingsRepository, permissionsRepository);
        }

        public async Task<IEnumerable<Discussion>> GetDiscussions(int itemId, int projectId)
        {
            var discussionsPrm = new DynamicParameters();
            discussionsPrm.Add("@ItemId", itemId);

            var discussions = (await _connectionWrapper.QueryAsync<Discussion>("GetItemDiscussions", discussionsPrm, commandType: CommandType.StoredProcedure)).ToList();
            var discussionStates = (await GetItemDiscussionStates(itemId)).ToDictionary(k => k.DiscussionId);
            var areEmailDiscussionsEnabled = await _mentionHelper.AreEmailDiscussionsEnabled(projectId);

            await InitializeCommentsProperties(discussions, areEmailDiscussionsEnabled, (discussion) => {
                var discussionState = (DiscussionState)null;
                if (discussionStates.TryGetValue(discussion.DiscussionId, out discussionState))
                {
                    discussion.IsClosed = discussionState.IsClosed;
                    discussion.Status = discussionState.Status;
                    var lastEditedOn = DateTime.SpecifyKind(discussionState.LastEditedOn, DateTimeKind.Utc);
                    discussion.LastEditedOn = Max(discussion.LastEditedOn, lastEditedOn);
                }
            });

            return discussions.OrderByDescending(d => d.LastEditedOn);
        }

        public async Task<IEnumerable<Reply>> GetReplies(int discussionId, int projectId)
        {
            var repliesPrm = new DynamicParameters();
            repliesPrm.Add("@DiscussionId", discussionId);

            var replies = (await _connectionWrapper.QueryAsync<Reply>("GetItemReplies", repliesPrm, commandType: CommandType.StoredProcedure)).ToList();
            var areEmailDiscussionsEnabled = await _mentionHelper.AreEmailDiscussionsEnabled(projectId);

            await InitializeCommentsProperties(replies, areEmailDiscussionsEnabled);

            return replies.OrderBy(r => r.LastEditedOn);
        }

        public Task<bool> IsDiscussionDeleted(int discussionId)
        {
            var discussionsPrm = new DynamicParameters();
            discussionsPrm.Add("@DiscussionId", discussionId);

            return _connectionWrapper.ExecuteScalarAsync<bool>("IsDiscussionDeleted", discussionsPrm, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<DiscussionState>> GetItemDiscussionStates(int itemId)
        {
            var discussionsPrm = new DynamicParameters();
            discussionsPrm.Add("@ItemId", itemId);

            return await _connectionWrapper.QueryAsync<DiscussionState>("GetItemDiscussionStates", discussionsPrm, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<ThreadStatus>> GetThreadStatusCollection(int projectId)
        {
            var itemPrm = new DynamicParameters();
            itemPrm.Add("@projectId", projectId);
            try
            {
                var result = await _connectionWrapper.QueryAsync<ThreadStatus>("GetThreadStatusCollection", itemPrm, commandType: CommandType.StoredProcedure);

                return result;
            }
            catch(Exception e)
            {
                string msg = e.Message;
            }
            return null;
        }

        private async Task InitializeCommentsProperties<T>(IEnumerable<T> comments, bool areEmailDiscussionsEnabled, Action<T> onCommentInit = null)
            where T : CommentBase
        {
            if (!comments.Any())
            {
                return;
            }
            var userIds = new HashSet<int>(comments.Select(d => d.UserId));
            var userInfos = (await _sqlUsersRepository.GetUserInfos(userIds)).ToDictionary(u => u.UserId);
            foreach (var comment in comments)
            {
                var userInfo = (UserInfo)null;
                if (userInfos.TryGetValue(comment.UserId, out userInfo))
                {
                    comment.DisplayName = userInfo.DisplayName;
                }
                comment.LastEditedOn = DateTime.SpecifyKind(comment.LastEditedOn, DateTimeKind.Utc);
                comment.IsGuest = userInfo.IsGuest;
                comment.Comment = await _mentionHelper.ProcessComment(comment.Comment, areEmailDiscussionsEnabled);

                onCommentInit?.Invoke(comment);
            }
        }

        private static DateTime Max(DateTime val1, DateTime val2)
        {
            return (val1 >= val2) ? val1 : val2;
        }

        public Task<bool> AreEmailDiscussionsEnabled(int projectId)
        {
            return _mentionHelper.AreEmailDiscussionsEnabled(projectId);
        }

    }
}