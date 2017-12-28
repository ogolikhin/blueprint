using System.Collections.Generic;
using ServiceLibrary.Repositories;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using System.Net;
using ServiceLibrary.Exceptions;
using Dapper;
using System.Linq;
using System;

namespace SearchEngineLibrary.Repository
{
    public class SearchEngineRepository: ISearchEngineRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;        

        public SearchEngineRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SearchEngineRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;           
        }

        public async Task<IEnumerable<int>> GetChildrenArtifactIdsByCollectionId(int scopeId, Pagination pagination, ScopeType scopeType, bool includeDraft, int userId)
        {
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@userId", userId);
            dynamicParameters.Add("@itemId", scopeId);
            ArtifactBasicDetails artifactBasicDetails = (await _connectionWrapper.QueryAsync<ArtifactBasicDetails>(
                "GetArtifactBasicDetails", dynamicParameters, commandType: CommandType.StoredProcedure)).FirstOrDefault();

            if (artifactBasicDetails == null)
            {
                string errorMessage = I18NHelper.FormatInvariant("Item (Id:{0}) is not found.", scopeId);
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }                                    

            if (artifactBasicDetails.PrimitiveItemTypePredefined != (int)ItemTypePredefined.ArtifactCollection)
            {
                throw new NotImplementedException(ErrorMessages.NotImplementedForNotCollection);
            }

            if (scopeType == ScopeType.Descendants)
            {
                throw new NotImplementedException(ErrorMessages.NotImplementedForDescendantsScopeType);
            }
            
            return await _connectionWrapper.QueryAsync<int>(
                @"SELECT DISTINCT([ArtifactId]) FROM [dbo].[SearchItems]", commandType:CommandType.Text);
        }
    }
}