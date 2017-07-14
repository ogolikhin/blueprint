using ArtifactStore.Models.VersionControl;
using ServiceLibrary.Models.VersionControl;
using System.Data;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories.VersionControl
{
    public class SqlPublishCollectionAssignmentsRepository : SqlPublishRepository, IPublishRepository
    {
        protected override string MarkAsLatestStoredProcedureName { get; } = "";
        protected override string DeleteVersionsStoredProcedureName { get; } = "";
        protected override string CloseVersionsStoredProcedureName { get; } = "";
        protected override string GetDraftAndLatestStoredProcedureName { get; } = "";

        public async Task Execute(int revisionId, PublishParameters parameters, PublishEnvironment environment, IDbTransaction transaction = null)
        {
            await Task.Run(() => { });
        }
    }
}