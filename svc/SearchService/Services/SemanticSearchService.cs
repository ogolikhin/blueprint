using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SearchService.Helpers.SemanticSearch;
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
        Task<SuggestionsSearchResult> GetSemanticSearchSuggestions(SemanticSearchSuggestionParameters parameters, GetSemanticSearchSuggestionsAsyncDelegate getSuggestionsAsyncDelegate);
    }
    public class SemanticSearchService: ISemanticSearchService
    {
        private IArtifactPermissionsRepository _artifactPermissionsRepository;
        private ISemanticSearchRepository _semanticSearchRepository;
        private IUsersRepository _usersRepository;
        private IArtifactRepository _artifactRepository;

        public SemanticSearchService() : this(
                new SemanticSearchRepository(),
                new SqlArtifactPermissionsRepository(new SqlConnectionWrapper(WebApiConfig.BlueprintConnectionString)),
                new SqlUsersRepository(new SqlConnectionWrapper(WebApiConfig.BlueprintConnectionString)), 
                new ArtifactRepository(new SqlConnectionWrapper(WebApiConfig.BlueprintConnectionString)))
        {
            
        }
        public SemanticSearchService(
            ISemanticSearchRepository semanticSearchRepository, 
            IArtifactPermissionsRepository artifactPermissionRepository,
            IUsersRepository usersRepository,
            IArtifactRepository artifactRepository)
        {
            _artifactPermissionsRepository = artifactPermissionRepository;
            _semanticSearchRepository = semanticSearchRepository;
            _usersRepository = usersRepository;
            _artifactRepository = artifactRepository;
        }

        public async Task<SuggestionsSearchResult> GetSemanticSearchSuggestions(
            SemanticSearchSuggestionParameters parameters,
            GetSemanticSearchSuggestionsAsyncDelegate getSuggestionsAsyncDelegate)
        {
            var artifactId = parameters.ArtifactId;
            var userId = parameters.UserId;
            if (artifactId <= 0)
            {
                throw new BadRequestException("Please specify a valid artifact id");
            }

            var artifactDetails = await _artifactRepository.GetArtifactBasicDetails(artifactId, userId);
            if (artifactDetails == null)
            {
                throw new ResourceNotFoundException(
                    I18NHelper.FormatInvariant("Artifact Id {0} is not found", artifactId), ErrorCodes.ArtifactNotFound);
            }
            if (artifactDetails.LatestDeleted || artifactDetails.DraftDeleted)
            {
                throw new ResourceNotFoundException(
                    I18NHelper.FormatInvariant("Artifact Id {0} is deleted", artifactId), ErrorCodes.ArtifactNotFound);
            }

            var itemTypePredefined = (ItemTypePredefined) artifactDetails.PrimitiveItemTypePredefined;

            if (isInvalidSemanticSearchArtifactType(itemTypePredefined))
            {
                throw new BadRequestException(
                    I18NHelper.FormatInvariant(
                        $"Artifact type '{itemTypePredefined}' is not supported for semantic search"));
            }

            if (artifactDetails.ArtifactId != artifactId && artifactDetails.ItemId == artifactId)
            {
                throw new BadRequestException("Subartifacts are not supported for semantic search");
            }

            var currentProject =
                (await _artifactRepository.GetProjectNameByIdsAsync(new[] {artifactDetails.ProjectId}))
                    .FirstOrDefault();

            var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(new[] {artifactId}, userId);

            RolePermissions permission;
            if (!permissions.TryGetValue(artifactId, out permission) || !permission.HasFlag(RolePermissions.Read))
            {
                throw new AuthorizationException("User is not authorized to view artifact");
            }

            var suggestionsSearchResult = new SuggestionsSearchResult();
            suggestionsSearchResult.SourceId = artifactId;
            suggestionsSearchResult.SourceProjectName = currentProject?.Name;

            var isInstanceAdmin = await _usersRepository.IsInstanceAdmin(false, userId);
            var accessibleProjectIds = isInstanceAdmin
                ? new List<int>()
                : await _semanticSearchRepository.GetAccessibleProjectIds(userId);

            var searchEngineParameters = new SearchEngineParameters(artifactId, userId, isInstanceAdmin,
                accessibleProjectIds.ToHashSet());

            var suggestedArtifactResults = await getSuggestionsAsyncDelegate(searchEngineParameters);

            var artifactIds = suggestedArtifactResults.Select(s => s.Id);

            var resultArtifactPermissions = await _artifactPermissionsRepository.GetArtifactPermissions(artifactIds, userId);

            suggestedArtifactResults.ForEach((artifact) =>
            {
                if (resultArtifactPermissions.ContainsKey(artifact.Id))
                {
                    artifact.HasReadPermission = resultArtifactPermissions[artifact.Id].HasFlag(RolePermissions.Read);
                }
            });

            //Get list of some basic artifact details from the list of returned ids.
            suggestionsSearchResult.Items = suggestedArtifactResults;

            return suggestionsSearchResult;
        }

        private bool isInvalidSemanticSearchArtifactType(ItemTypePredefined itemTypePredefined)
        {
            return !itemTypePredefined.IsRegularArtifactType() || itemTypePredefined.IsProjectOrFolderArtifactType() ||
                   itemTypePredefined.IsSubArtifactType();
        }
    }
}