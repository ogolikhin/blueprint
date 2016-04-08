using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models;
using Dapper;
using ServiceLibrary.Repositories;
using System.Data;
using System.Linq;
using AdminStore.Helpers;

namespace AdminStore.Repositories
{
    public class SqlInstanceRepository : ISqlInstanceRepository
    {
        internal readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlInstanceRepository()
            : this(new SqlConnectionWrapper(WebApiConfig.RaptorMain))
        {
        }

        internal SqlInstanceRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<InstanceItem> GetInstanceFolderAsync(int folderId)
        {
            if (folderId < 1)
                throw new ArgumentOutOfRangeException(nameof(folderId));

            var prm = new DynamicParameters();
            prm.Add("@folderId", folderId);
            var folder = (await _connectionWrapper.QueryAsync<InstanceItem>("GetInstanceFolderById", prm, commandType: CommandType.StoredProcedure))?.FirstOrDefault();
            if(folder == null)
                throw new ResourceNotFoundException(string.Format("Instance Folder (Id:{0}) is not found.", folderId), ErrorCodes.ResourceNotFound);

            folder.Type = InstanceItemTypeEnum.Folder;
            return folder;
        }

        public async Task<List<InstanceItem>> GetInstanceFolderChildrenAsync(int folderId, int userId)
        {
            if (folderId < 1)
                throw new ArgumentOutOfRangeException(nameof(folderId));
            if (userId < 1)
                throw new ArgumentOutOfRangeException(nameof(userId));

            var prm = new DynamicParameters();
            prm.Add("@folderId", folderId);
            prm.Add("@userId", userId);

            return  ((await _connectionWrapper.QueryAsync<InstanceItem>("GetInstanceFolderChildren", prm, commandType: CommandType.StoredProcedure))
                ?? Enumerable.Empty <InstanceItem>()).OrderBy(i => i.Type).ThenBy(i => i.Name).ToList();
        }
    }
}