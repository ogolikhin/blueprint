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

        public async Task<InstanceItem> GetInstanceFolderAsync(int id)
        {
            if (id < 1)
                throw new ArgumentOutOfRangeException(nameof(id));

            var prm = new DynamicParameters();
            prm.Add("@folderId", id);
            var folder = (await _connectionWrapper.QueryAsync<InstanceItem>("GetInstanceFolderById", prm, commandType: CommandType.StoredProcedure))?.FirstOrDefault();
            if(folder == null)
                throw new ResourceNotFoundException(string.Format("Instance Folder (Id:{0}) is not found.", id), ErrorCodes.ResourceNotFound);

            folder.Type = InstanceItemTypeEnum.Folder;
            return folder;
        }

        public async Task<List<InstanceItem>> GetInstanceFolderChildrenAsync(int id)
        {
            if (id < 1)
                throw new ArgumentOutOfRangeException(nameof(id));

            await Task.Delay(1);
            throw new NotImplementedException();
        }
    }
}