using System;
using System.Collections.Generic;
using System.Linq;
using FileStore.Models;
using System.Data.SqlClient;
using Dapper;
using System.Data;
using FileStore.Helpers;
using System.Threading.Tasks;

namespace FileStore.Repo
{
    public class SqlRepo : IRepo
    {
        public async Task<bool> AddFile(File file)
        {
            using (var conn = new SqlConnection(WebConfig.GetConnectionString("FileStoreDatabase")))
            {
                conn.Open();
                var parms = new DynamicParameters();
                parms.Add("@FileName", file.FileName);
                parms.Add("@FileType", file.FileType);
                parms.Add("@FileContent", file.FileContent);

                return await conn.ExecuteAsync("AddFile", parms, commandType: CommandType.StoredProcedure) > 0;
            }
        }

        public async Task<bool> DeleteFile(Guid fileId)
        {
            using (var conn = new SqlConnection(WebConfig.GetConnectionString("FileStoreDatabase")))
            {
                conn.Open();
                var parms = new DynamicParameters();
                parms.Add("@FileId", fileId);

                return await conn.ExecuteAsync("DeleteFile", parms, commandType: CommandType.StoredProcedure) > 0;
            }
        }

        public async Task<File> GetFile(Guid fileId)
        {
            using (var conn = new SqlConnection(WebConfig.GetConnectionString("FileStoreDatabase")))
            {
                conn.Open();
                var parms = new DynamicParameters();
                parms.Add("@FileId", fileId);

                IEnumerable<File> files = await conn.QueryAsync<File>("GetFile", parms, commandType: CommandType.StoredProcedure);

                if (files.Count<File>() == 0)
                {
                    return null;
                }
                return files.First();
            }
        }

        public async Task<File> GetFileInfo(Guid fileId)
        {
            using (var conn = new SqlConnection(WebConfig.GetConnectionString("FileStoreDatabase")))
            {
                conn.Open();
                var parms = new DynamicParameters();
                parms.Add("@FileId", fileId);

                IEnumerable<File> files = await conn.QueryAsync<File>("GetFileInfo", parms, commandType: CommandType.StoredProcedure);

                if (files.Count<File>() == 0)
                {
                    return null;
                }
                return files.First();
            }
        }
    }
}