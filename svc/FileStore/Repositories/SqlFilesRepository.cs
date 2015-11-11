using System;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using FileStore.Models;
using ServiceLibrary.Repositories;

namespace FileStore.Repositories
{
    public class SqlFilesRepository : IFilesRepository
    {
        internal readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlFilesRepository()
            : this(new SqlConnectionWrapper(new ConfigRepository().FileStoreDatabase))
        {
        }

        internal SqlFilesRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<Guid?> PostFile(File file)
        {
            var prm = new DynamicParameters();
            prm.Add("@FileName", file.FileName);
            prm.Add("@FileType", file.FileType);
            prm.Add("@FileContent", file.FileContent);
            prm.Add("@FileId", dbType: DbType.Guid, direction: ParameterDirection.Output);
            await _connectionWrapper.ExecuteAsync("PostFile", prm, commandType: CommandType.StoredProcedure);
            return prm.Get<Guid?>("FileId");
        }

        public async Task<File> HeadFile(Guid guid)
        {
            var prm = new DynamicParameters();
            prm.Add("@FileId", guid);
            return (await _connectionWrapper.QueryAsync<File>("HeadFile", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }
         
        public async Task<File> GetFile(Guid guid)
        {
            var prm = new DynamicParameters();
            prm.Add("@FileId", guid);
            return (await _connectionWrapper.QueryAsync<File>("GetFile", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        public async Task<Guid?> DeleteFile(Guid guid)
        {
            var prm = new DynamicParameters();
            prm.Add("@FileId", guid);
            prm.Add("@DeletedFileId", dbType: DbType.Guid, direction: ParameterDirection.Output);
            await _connectionWrapper.ExecuteAsync("DeleteFile", prm, commandType: CommandType.StoredProcedure);
            return prm.Get<Guid?>("DeletedFileId");
        }
    }
}
