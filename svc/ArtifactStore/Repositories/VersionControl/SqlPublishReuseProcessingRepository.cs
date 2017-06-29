using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using ArtifactStore.Repositories.Reuse;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.VersionControl;

namespace ArtifactStore.Repositories.VersionControl
{
    public class SqlPublishReuseProcessingRepository : SqlPublishRepository, IPublishRepository
    {
        protected override string MarkAsLatestStoredProcedureName { get; } = "";
        protected override string DeleteVersionsStoredProcedureName { get; } = "";
        protected override string CloseVersionsStoredProcedureName { get; } = "";
        protected override string GetDraftAndLatestStoredProcedureName { get; } = "";

        private readonly IReuseRepository _reuseRepository;
        private readonly ISensitivityCommonHelper _sensitivityCommonHelper;

        public SqlPublishReuseProcessingRepository() : this(new ReuseRepository(), new SensitivityCommonHelper())
        {
            
        }

        public SqlPublishReuseProcessingRepository(IReuseRepository reuseRepository,
            ISensitivityCommonHelper sensitivityCommonHelper)
        {
            _reuseRepository = reuseRepository;
            _sensitivityCommonHelper = sensitivityCommonHelper;
        }

        public async Task Execute(int revisionId, PublishParameters parameters, PublishEnvironment environment, IDbTransaction transaction = null)
        {
            var affectedArtifacts = environment.GetAffectedArtifacts().Where(a => {
                SqlItemInfo info;
                if (environment.ArtifactStates.TryGetValue(a, out info))
                {
                    return info.PrimitiveItemTypePredefined.IsAvailableForSensitivityCalculations();
                }
                return true;
            }).ToList();
            
            var affectedStandardArtifacts = await _sensitivityCommonHelper.FilterInsensitiveItems(affectedArtifacts, 
                environment.SensitivityCollector, 
                _reuseRepository);
            await MarkReuseLinksOutOfSync(affectedStandardArtifacts, environment, transaction);
        }
    }
}