using ArtifactStore.Helpers;
using ArtifactStore.Models.VersionControl;
using Dapper;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.Reuse;

namespace ArtifactStore.Repositories.VersionControl
{
    public class SqlPublishPropertiesRepository : SqlPublishRepository, IPublishRepository
    {
        protected override string MarkAsLatestStoredProcedureName { get; } = "MarkAsLatestPropertyVersions";
        protected override string DeleteVersionsStoredProcedureName { get; } = "RemovePropertyVersions";
        protected override string CloseVersionsStoredProcedureName { get; } = "ClosePropertyVersions";
        protected override string GetDraftAndLatestStoredProcedureName { get; } = "GetDraftAndLatestPropertyVersions";

        public async Task Execute(int revisionId, PublishParameters parameters, PublishEnvironment environment, IDbTransaction transaction = null)
        {
            var properties = await GetDraftAndLatest<SqlDraftAndLatestProperty>(parameters.UserId, parameters.AffectedArtifactIds, transaction);

            if (properties.Count == 0)
            {
                return;
            }

            var deleteVersionsIds = new HashSet<int>();
            var closeVersionIds = new HashSet<int>();
            var markAsLatestVersionIds = new HashSet<int>();

            foreach (var property in properties)
            {
                if (property.DraftDeleted)
                {
                    deleteVersionsIds.Add(property.DraftVersionId);

                    if (property.LatestVersionId.HasValue)
                    {
                        closeVersionIds.Add(property.LatestVersionId.Value);
                        
                        //TODO: reviews are not handled currently
                        //_reviewProcessor.ProcessArtifactReviewPackageChanges(property, environment);

                        RegisterPropertyModification(environment.SensitivityCollector, property);
                    }

                    environment.AddAffectedArtifact(property.ArtifactId);
                }
                else
                {
                    if (IsChanged(property))
                    {
                        markAsLatestVersionIds.Add(property.DraftVersionId);

                        if (property.LatestVersionId.HasValue)
                        {
                            closeVersionIds.Add(property.LatestVersionId.Value);
                        }

                        //TODO: reviews are not handled currently
                        //_reviewProcessor.ProcessArtifactReviewPackageChanges(property, environment);

                        environment.AddAffectedArtifact(property.ArtifactId);
                        RegisterPropertyModification(environment.SensitivityCollector, property);
                    }
                    else
                    {
                        deleteVersionsIds.Add(property.DraftVersionId);
                    }
                }
            }

            await ClosePropertyVersions(closeVersionIds, environment.RevisionId, transaction);
            await CloseVersionsForCrossProjectMovedArtifacts(environment.GetArtifactsMovedAcrossProjects(parameters.AffectedArtifactIds), 
                environment.RevisionId, 
                transaction);
            await DeleteVersions(deleteVersionsIds, transaction);
            await MarkAsLatest(markAsLatestVersionIds, environment.RevisionId, transaction);
        }

        private async Task ClosePropertyVersions(ISet<int> closeVersionIds, int revisionId, IDbTransaction transaction)
        {
            if (closeVersionIds.Count == 0)
            {
                return;
            }

            var param = new DynamicParameters();
            param.Add("@revisionId", revisionId);
            param.Add("@versionIds", SqlConnectionWrapper.ToDataTable(closeVersionIds));

            if (transaction == null)
            {
                await ConnectionWrapper.ExecuteAsync(CloseVersionsStoredProcedureName, param,
                commandType: CommandType.StoredProcedure);
                return;
            }

            await transaction.Connection.ExecuteAsync(CloseVersionsStoredProcedureName, param, transaction,
                commandType: CommandType.StoredProcedure);

            //TODO: Fix this assert
            //Log.Assert(updatedRowsCount == closeVersionIds.Count, "Publish: Some property versions are not closed");
        }
        
        private void RegisterPropertyModification(ReuseSensitivityCollector sensitivityCollector, SqlDraftAndLatestProperty property)
        {
            if (IsSubArtifactChange(property))
            {
                sensitivityCollector.RegisterArtifactModification(property.ArtifactId,
                    ItemTypeReuseTemplateSetting.Subartifacts);
            }
            else
            {
                sensitivityCollector.RegisterArtifactPropertyModification(property.ArtifactId,
                    property.PropertyTypeId, property.PropertyTypePredefined);
            }
        }

        private bool IsSubArtifactChange(SingleArtifactData property)
        {
            return property.ArtifactId != property.ItemId;
        }

        private bool IsChanged(SqlDraftAndLatestProperty item)
        {
            return item.Changed;
        }

        private async Task CloseVersionsForCrossProjectMovedArtifacts(IEnumerable<int> artifactIds, int revisionId, IDbTransaction transaction)
        {
            var aids = artifactIds.ToArray();

            if (aids.IsEmpty())
            {
                return;
            }

            var param = new DynamicParameters();
            param.Add("@revisionId", revisionId);
            param.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(aids));

            if (transaction == null)
            {
                await ConnectionWrapper.ExecuteAsync("CloseAllPropertyVersions", param,
                commandType: CommandType.StoredProcedure);
                return;
            }

            await transaction.Connection.ExecuteAsync("CloseAllPropertyVersions", param, transaction,
                commandType: CommandType.StoredProcedure);
        }
    }

    public class SqlDraftAndLatestProperty : SingleArtifactData
    {
        public PropertyTypePredefined PropertyTypePredefined { get; set; }

        public int PropertyTypeId { get; set; }

        public bool Changed { get; set; }

        public int DraftProjectId { get; set; }
        public int? LatestProjectId { get; set; }

        public PropertyPrimitiveType DraftPrimitiveType { get; set; }
        public PropertyPrimitiveType? LatestPrimitiveType { get; set; }
    }
}