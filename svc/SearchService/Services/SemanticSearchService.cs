﻿using System.Collections.Generic;
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
        Task<SuggestionsSearchResult> GetSemanticSearchSuggestions(SemanticSearchSuggestionParameters parameters);
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

        public async Task<SuggestionsSearchResult> GetSemanticSearchSuggestions(SemanticSearchSuggestionParameters parameters)
        {
            var artifactId = parameters.ArtifactId;
            var userId = parameters.UserId;
            if (artifactId <= 0)
            {
                throw new BadRequestException("Please specify a valid artifact id");
            }

            var artifactDetails = await _sqlArtifactRepository.GetArtifactBasicDetails(artifactId, userId);
            if (artifactDetails == null)
            {
                throw new ResourceNotFoundException(I18NHelper.FormatInvariant("Artifact Id {0} is not found", artifactId), ErrorCodes.ArtifactNotFound);
            }

            var itemTypePredefined = (ItemTypePredefined) artifactDetails.PrimitiveItemTypePredefined;

            if (!itemTypePredefined.IsRegularArtifactType() || itemTypePredefined.IsProjectOrFolderArtifactType())
            {
                throw new BadRequestException(I18NHelper.FormatInvariant($"Artifact type '{itemTypePredefined}' is not supported for suggestions"));
            }

            var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(new[] {artifactId}, userId);

            RolePermissions permission;
            if (!permissions.TryGetValue(artifactId, out permission) || !permission.HasFlag(RolePermissions.Read))
            {
                throw new AuthorizationException("User is not authorized to view artifact");
            }

            var suggestionsSearchResult = new SuggestionsSearchResult();
            suggestionsSearchResult.SourceId = artifactId;

            var isInstanceAdmin = await _usersRepository.IsInstanceAdmin(false, userId);
            var accessibleProjectIds = isInstanceAdmin ? new List<int>() : await _semanticSearchRepository.GetAccessibleProjectIds(userId);

            var suggestedArtifactIds = await _semanticSearchRepository.GetSuggestedArtifacts(artifactId, isInstanceAdmin, accessibleProjectIds);
            
            // TEMPORARY ADDED artifactId
            var artifactsInfos = await _semanticSearchRepository.GetSuggestedArtifactDetails(suggestedArtifactIds, userId, artifactId);

            //Get list of some basic artifact details from the list of returned ids.
            suggestionsSearchResult.Items = artifactsInfos;

            return suggestionsSearchResult;
        }
    }
}