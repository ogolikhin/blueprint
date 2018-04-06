using System;
using System.Data;
using System.Threading.Tasks;
using SearchEngineLibrary.Model;
using SearchEngineLibrary.Repository;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Repositories;

namespace SearchEngineLibrary.Service
{
    public class SearchEngineService : ISearchEngineService
    {
        private readonly ISearchEngineRepository _searchEngineRepository;
        private readonly IArtifactRepository _sqlArtifactRepository;

        public SearchEngineService() : this(new SearchEngineRepository(), new SqlArtifactRepository())
        {
        }

        internal SearchEngineService(ISearchEngineRepository searchEngineRepository, IArtifactRepository sqlArtifactRepository)
        {
            _searchEngineRepository = searchEngineRepository;
            _sqlArtifactRepository = sqlArtifactRepository;
        }

        public async Task<SearchArtifactsResult> Search(int scopeId, int projectId, Pagination pagination, ScopeType scopeType, bool includeDrafts, int userId, IDbTransaction transaction = null)
        {
            var artifactBasicDetails = await _sqlArtifactRepository.GetArtifactBasicDetails(scopeId, userId, transaction);

            if (artifactBasicDetails == null)
            {
                var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.ArtifactNotFound, scopeId);
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }

            if (artifactBasicDetails.PrimitiveItemTypePredefined != (int)ItemTypePredefined.ArtifactCollection)
            {
                throw new NotImplementedException(ErrorMessages.NotImplementedForNonCollectionArtifact);
            }

            if (scopeType == ScopeType.Descendants)
            {
                throw new NotImplementedException(ErrorMessages.NotImplementedForDescendantsScopeType);
            }

            return await _searchEngineRepository.GetCollectionContentSearchArtifactResults(scopeId, projectId, pagination, includeDrafts, userId, transaction);
        }
    }
}
