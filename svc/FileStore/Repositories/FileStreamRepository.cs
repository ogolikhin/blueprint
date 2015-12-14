using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using File = FileStore.Models.File;

namespace FileStore.Repositories
{
    public class FileStreamRepository : IFileStreamRepository
    {
        private readonly IConfigRepository _configRepository;
 
        private const int CommandTimeout = 60;

        public FileStreamRepository() : this(ConfigRepository.Instance)
        {

        }

        internal FileStreamRepository(IConfigRepository configRepository)
        {
            _configRepository = configRepository;
        }

        public DbConnection CreateConnection()
        {
            // create a connection for operations that require holding an open connection to the db
            return new SqlConnection(_configRepository.FileStreamDatabase);
        }

        public bool IsDatabaseAvailable()
        {
             // check that we have a connection string for the legacy db
            return (string.IsNullOrEmpty(_configRepository.FileStreamDatabase)) ? false : true;
        }

        public bool FileExists(Guid fileId)
        {
            long fileSize = 0;

            // check if the legacy database is configured 
            // if it is not available then return false 

            if (IsDatabaseAvailable() == false)
            {
                return false;
            }
            // returns true if the file is found in the legacy database 
            // returns false not found in the legacy database

            if (fileId == Guid.Empty)
            {
                throw new ArgumentException("fileId param is empty.");
            }

            fileSize = GetFileSize(fileId);

            // if there is no file content assume that the file does not exist in the legacy db
            return fileSize == 0 ? false : true;

        }

        public File GetFileHead(Guid fileId)
        {
            // return a File object with the file info  or null if the file is
            // not found in the legacy database

            if (fileId == Guid.Empty)
            {
                throw new ArgumentException("fileId param is empty.");
            }

            File file = new File();
 
            file.FileId = fileId;
            file.FileSize = GetFileSize(fileId);
            file.FileName = GetFileName(fileId);
            file.FileType = GetFileType(fileId);
            file.IsLegacyFile = true;

            // if there is no file content assume that the file does not exist in the legacy db
            return file.FileSize == 0 ? null : file; 
             
        }

        public long GetFileSize(Guid fileId)
        {
            long fileSize = 0;

            using (var sqlConnection = CreateConnection())
            {
                using (SqlCommand cmd = (sqlConnection as SqlConnection).CreateCommand())
                {
                    sqlConnection.Open();

                    // get file size by checking the file content
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT @pLength = DATALENGTH([Content]) FROM [dbo].[Files] WHERE ([FileGuid] = @pFileGuid);";
                    cmd.Parameters.AddWithValue("@pFileGuid", fileId);
                    SqlParameter pLength = cmd.Parameters.AddWithValue("@pLength", 0L);
                    pLength.Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();

                    fileSize = (pLength.Value is DBNull) ? 0L : (Int64)pLength.Value;
                }
            }
            return fileSize; 
        }

        private string _fileType;
        public string GetFileType(Guid fileId)
        {
            if (!string.IsNullOrWhiteSpace(_fileType))
            {
                return _fileType;
            }

            using (var sqlConnection = CreateConnection())
            {
                using (SqlCommand cmd = (sqlConnection as SqlConnection).CreateCommand())
                {
                    sqlConnection.Open();
                    // get file type
                    var pType = GetFileTypeFromAttachmentVersions(fileId, cmd);
                    _fileType = (pType.Value is DBNull) ? string.Empty : (string)pType.Value;
                    if (string.IsNullOrWhiteSpace(_fileType))
                    {
                        var fileName = GetFileName(fileId);
                        if (!string.IsNullOrWhiteSpace(fileName))
                        {
                            var fileInfo = new FileInfo(fileName);
                            _fileType = fileInfo.Extension;
                        }
                        else
                        {
                            _fileType = string.Empty;
                        }
                    }
                }
            }
            return _fileType;
        }

        private SqlParameter GetFileTypeFromAttachmentVersions(Guid fileId, SqlCommand cmd)
        {
            cmd.Parameters.Clear();
            cmd.CommandText =
                "SELECT TOP 1 @pType = [Type] FROM [dbo].[AttachmentVersions] WHERE ([FileGuid] = @pFileGuid);";
            cmd.Parameters.AddWithValue("@pFileGuid", fileId);
            SqlParameter pType = cmd.Parameters.Add("@pType", SqlDbType.NVarChar, Int32.MaxValue);
            pType.Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();
            return pType;
        }

        private string _fileName;
        public string GetFileName(Guid fileId)
        {
            if (!string.IsNullOrWhiteSpace(_fileName))
            {
                return _fileName;
            }

            using (var sqlConnection = CreateConnection())
            {
                using (SqlCommand cmd = (sqlConnection as SqlConnection).CreateCommand())
                {
                    sqlConnection.Open();
                    
                    _fileName = GetFileNameFromAttachmentVersions(fileId, cmd);
                    
                    if (string.IsNullOrWhiteSpace(_fileName))
                    {
                        _fileName = GetFileNameFromTemplates(fileId, cmd);
                    }
                }
            }
            return _fileName;
        }

        private static string GetFileNameFromAttachmentVersions(Guid fileId, SqlCommand cmd)
        {
            cmd.Parameters.Clear();
            cmd.CommandText =
                "SELECT TOP 1 @pName = [Name] FROM [dbo].[AttachmentVersions] WHERE ([FileGuid] = @pFileGuid);";
            cmd.Parameters.AddWithValue("@pFileGuid", fileId);
            SqlParameter pName = cmd.Parameters.Add("@pName", SqlDbType.NVarChar, Int32.MaxValue);
            pName.Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();
            return (pName.Value is DBNull) ? string.Empty : (string) pName.Value;
        }

        private static string GetFileNameFromTemplates(Guid fileId, SqlCommand cmd)
        {
            cmd.Parameters.Clear();
            cmd.CommandText = "SELECT TOP 1 @pName = [Path] FROM [dbo].[Templates] WHERE ([FileGuid] = @pFileGuid);";
            cmd.Parameters.AddWithValue("@pFileGuid", fileId);
            SqlParameter pName = cmd.Parameters.Add("@pName", SqlDbType.NVarChar, Int32.MaxValue);
            pName.Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();
            return (pName.Value is DBNull) ? string.Empty : (string)pName.Value;
        }

        public byte[] ReadChunkContent(DbConnection dbConnection, Guid fileId, long count, long position)
        {
            // Note: this method may be called hundreds of times to retrieve chunk records if the 
            // stored file is large. It will reuse the open database connection that is passed 
            // in as a parameter.

            // Note: After all the read operations are finsihed the dbConnection object must be closed
            // and disposed by the calling procedure.

            byte[] content = null;

            if (dbConnection == null || dbConnection.State == ConnectionState.Closed)
            {
                throw new ArgumentNullException("The database connection must be open prior to use.");
            }

            using (SqlCommand cmd = (dbConnection as SqlConnection).CreateCommand())
            {
                cmd.CommandTimeout = CommandTimeout;
 
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT @pContent = SUBSTRING([Content], @pOffset, @pCount ) FROM [dbo].[Files] WHERE ([FileGuid] = @pFileGuid);";
                cmd.Parameters.AddWithValue("@pFileGuid", fileId);
                SqlParameter offsetParam = cmd.Parameters.AddWithValue("@pOffset", 0L);
                SqlParameter countParam = cmd.Parameters.AddWithValue("@pCount", 0L);
                SqlParameter contentParam = cmd.Parameters.Add("@pContent", SqlDbType.VarBinary, int.MaxValue);
                contentParam.Direction = ParameterDirection.Output;

                offsetParam.Value = position + 1;
                countParam.Value = count;
                cmd.ExecuteNonQuery();

                if (contentParam.Value is DBNull)
                {
                    // return null if no bytes read 
                    content = null; 
                }
                else
                {
                    content = (byte[])contentParam.Value;
                }
            }

            return content;     
        }
 
    }
}
