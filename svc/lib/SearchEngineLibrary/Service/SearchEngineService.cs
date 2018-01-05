using System.Collections.Generic;
using System.Threading.Tasks;
using SearchEngineLibrary.Repository;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Repositories;
using ServiceLibrary.Helpers;
using ServiceLibrary.Exceptions;
using System;
using SearchEngineLibrary.Model;

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

        public async Task<SearchArtifactsResult> SearchArtifactIds(int scopeId, Pagination pagination, ScopeType scopeType, bool includeDraft, int userId)
        {           
            var artifactBasicDetails = await _sqlArtifactRepository.GetArtifactBasicDetails(scopeId, userId);
                      
            if (artifactBasicDetails == null)
            {
                var errorMessage = I18NHelper.FormatInvariant(ErrorMessages.ArtifactNotFound, scopeId);
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }

            if (artifactBasicDetails.PrimitiveItemTypePredefined != (int)ItemTypePredefined.ArtifactCollection)
            {
                throw new NotImplementedException(ErrorMessages.NotImplementedForNotCollection);
            }

            return await _searchEngineRepository.GetArtifactIds(scopeId, pagination, scopeType, includeDraft, userId);
        }
    }
}
