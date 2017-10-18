using ArtifactStore.Models.VersionControl;
using Dapper;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.VersionControl;
using System;
using System.Data;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories.VersionControl
{
    public class SqlJournalPublishRepository : SqlPublishRepository, IPublishRepository
    {
        protected override string MarkAsLatestStoredProcedureName { get; } = "";
        protected override string DeleteVersionsStoredProcedureName { get; } = "";
        protected override string CloseVersionsStoredProcedureName { get; } = "";
        protected override string GetDraftAndLatestStoredProcedureName { get; } = "";

        public async Task Execute(int revisionId, PublishParameters parameters, PublishEnvironment environment, IDbTransaction transaction = null)
        {
            await AddArtifactChanges(parameters, environment, transaction);
        }

        public async Task AddArtifactChanges(PublishParameters parameters, PublishEnvironment environment, IDbTransaction transaction = null)
        {
            var affectedArtifacts = environment.GetAffectedArtifacts();
            foreach (var item in environment.ArtifactStates)
            {
                ActionType action;

                if (environment.DeletedArtifactIds.Contains(item.Key))
                {
                    action = ActionType.Removed;
                }
                else if (affectedArtifacts.Contains(item.Key))
                {
                    action = item.Value.HasNeverBeenPublished ? ActionType.Added : ActionType.Updated;
                }
                else
                {
                    // Skip not changed artifacts
                    continue;
                }

                try
                {
                    await AddEntry(
                        new SqlJournalEntry
                        {
                            RevisionID = environment.RevisionId,
                            TimeStamp = environment.Timestamp,
                            UserID = parameters.UserId,
                            ItemID = item.Value.ItemId,
                            ElementType = (int)ElementType.Artifact,
                            ActionType = (int)action
                        });
                }
                catch (Exception)
                {
                    // swallow
                }
            }
        }

        private async Task AddEntry(SqlJournalEntry sqlJournalEntry)
        {
            // no need to check the session expiration. this call should not affect any service that uses it
            try
            {
                var param = new DynamicParameters();
                param.Add("@pRevisionId", sqlJournalEntry.RevisionID);
                param.Add("@pUserId", sqlJournalEntry.UserID);
                param.Add("@pTimestamp", sqlJournalEntry.TimeStamp);
                param.Add("@pElementType", sqlJournalEntry.ElementType);
                param.Add("@pItemId", sqlJournalEntry.ItemID);
                param.Add("@pSubItemId", sqlJournalEntry.SubItemID);
                param.Add("@pThreadId", sqlJournalEntry.ThreadID);
                param.Add("@pCommentId", sqlJournalEntry.CommentID);
                param.Add("@pActionType", sqlJournalEntry.ActionType);
                param.Add("@pSubActionType", sqlJournalEntry.SubActionType);
                param.Add("@pAdditionalInfoNum", sqlJournalEntry.AdditionalInfoNum);
                param.Add("@pAdditionalInfoTxt", sqlJournalEntry.AdditionalInfoTxt);

                await ConnectionWrapper.ExecuteAsync("JournalEntries", param,
                            commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {
                // swallow

            }
        }
    }
}