using System.Collections.Generic;
using System.Threading.Tasks;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace SearchService.Services
{
    public interface ISemanticSearchService
    {
        Task<SuggestionsSearchResult> Suggests(int id, int userId);
    }
    public class SemanticSearchService: ISemanticSearchService
    {
        private IArtifactPermissionsRepository _artifactPermissionsRepository;
        private ISemanticSearchRepository _semanticSearchRepository;
        private IUsersRepository _usersRepository;
        private ISqlArtifactRepository _sqlArtifactRepository;

        public SemanticSearchService() : this(
                new SemanticSearchRepository(),
                new SqlArtifactPermissionsRepository(new SqlConnectionWrapper(WebApiConfig.BlueprintConnectionString)),
                new SqlUsersRepository(new SqlConnectionWrapper(WebApiConfig.BlueprintConnectionString)), 
                new SqlArtifactRepository(new SqlConnectionWrapper(WebApiConfig.BlueprintConnectionString)))
        {
            
        }
        public SemanticSearchService(
            ISemanticSearchRepository semanticSearchRepository, 
            IArtifactPermissionsRepository artifactPermissionRepository,
            IUsersRepository usersRepository,
            ISqlArtifactRepository sqlArtifactRepository)
        {
            _artifactPermissionsRepository = artifactPermissionRepository;
            _semanticSearchRepository = semanticSearchRepository;
            _usersRepository = usersRepository;
            _sqlArtifactRepository = sqlArtifactRepository;
        }

        public async Task<SuggestionsSearchResult> Suggests(int id, int userId)
        {
            if (id <= 0)
            {
                throw new BadRequestException("Please specify a valid artifact id");
            }
            var artifactDetails = await _sqlArtifactRepository.GetArtifactBasicDetails(id, userId);
            var itemTypePredefined = (ItemTypePredefined) artifactDetails.PrimitiveItemTypePredefined;

            if (!itemTypePredefined.IsRegularArtifactType() || itemTypePredefined.IsProjectOrFolderArtifactType())
            {
                throw new BadRequestException(I18NHelper.FormatInvariant($"Artifact type '{itemTypePredefined.ToString()}' is not supported for suggestions"));
            }

            var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(new[] {id}, userId);

            RolePermissions permission = RolePermissions.None;
            if (!permissions.TryGetValue(id, out permission) || !permission.HasFlag(RolePermissions.Read))
            {
                throw new AuthorizationException("User is not authorized to view artifact");
            }

            var suggestionsSearchResult = new SuggestionsSearchResult();
            suggestionsSearchResult.SourceId = id;

            var isInstanceAdmin = await _usersRepository.IsInstanceAdmin(false, userId);
            var accessibleProjectIds = isInstanceAdmin ? new List<int>() : await _semanticSearchRepository.GetAccessibleProjectIds(userId);

            var suggestedArtifactIds = _semanticSearchRepository.GetSuggestedArtifacts(id, isInstanceAdmin, accessibleProjectIds);

            //Get list of some basic artifact details from the list of returned ids.
            //suggestionsSearchResult.SuggestedArtifacts.AddRange(suggestions);

            return suggestionsSearchResult;
        }
    }
}