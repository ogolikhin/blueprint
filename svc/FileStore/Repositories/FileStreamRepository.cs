using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using File = FileStore.Models.File;

namespace FileStore.Repositories
{
    public class FileStreamRepository : IFileStreamRepository
    {
        private readonly IConfigRepository _configRepository;
        private readonly IContentReadStream _contentReadStream;

        public FileStreamRepository() : this(ConfigRepository.Instance, new ContentReadStream())
        {

        }

        internal FileStreamRepository(IConfigRepository configRepository, IContentReadStream contentReadStream)
        {
            _configRepository = configRepository;
            _contentReadStream = contentReadStream;
        }

        public File GetFileHead(Guid fileId)
        {
            const int CommandTimeout = 60;

            // return a File object with the file info  or null if the file is
            // not found in the legacy database

            if (fileId == Guid.Empty)
            {
                throw new ArgumentException("fileId param is empty.");
            }

            File file = new File() { FileId = fileId };

            try
            {
                using (var sqlConnection = new SqlConnection(_configRepository.FileStreamDatabase))
                {
                    using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                    {
                        sqlConnection.Open();

                        // get file size by checking the file content
                        sqlCommand.CommandTimeout = CommandTimeout;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = "SELECT @pLength = DATALENGTH([Content]) FROM [dbo].[Files] WHERE ([FileGuid] = @pFileGuid);";
                        sqlCommand.Parameters.AddWithValue("@pFileGuid", fileId);
                        SqlParameter pLength = sqlCommand.Parameters.AddWithValue("@pLength", 0L);
                        pLength.Direction = ParameterDirection.Output;
                        sqlCommand.ExecuteNonQuery();

                        file.FileSize = (pLength.Value is DBNull) ? 0L : (Int64)pLength.Value;

                        if (file.FileSize == 0)
                        {
                            // if there is no file content assume that the file does not exist in the legacy db
                            return null;
                        }
                        // get file type
                        sqlCommand.Parameters.Clear();
                        sqlCommand.CommandText = "SELECT TOP 1 @pType = [Type] FROM [dbo].[AttachmentVersions] WHERE ([File_FileGuid] = @pFileGuid);";
                        sqlCommand.Parameters.AddWithValue("@pFileGuid", fileId);
                        SqlParameter pType = sqlCommand.Parameters.Add("@pType", SqlDbType.NVarChar, Int32.MaxValue);
                        pType.Direction = ParameterDirection.Output;
                        sqlCommand.ExecuteNonQuery();
                        file.FileType = (pType.Value is DBNull) ? string.Empty : (string)pType.Value;

                        // get file name 
                        sqlCommand.Parameters.Clear();
                        sqlCommand.CommandText = "SELECT TOP 1 @pName = [Name] FROM [dbo].[AttachmentVersions] WHERE ([File_FileGuid] = @pFileGuid);";
                        sqlCommand.Parameters.AddWithValue("@pFileGuid", fileId);
                        SqlParameter pName = sqlCommand.Parameters.Add("@pName", SqlDbType.NVarChar, Int32.MaxValue);
                        pName.Direction = ParameterDirection.Output;
                        sqlCommand.ExecuteNonQuery();
                        file.FileName = (pName.Value is DBNull) ? string.Empty : (string)pName.Value;
                    }

                }

            }
            catch (Exception e)
            {
                throw new InvalidOperationException(e.Message, e);
            }

            return file;
             
        }

        public Stream GetFileContent(Guid fileId)
        {
            // return a custom stream that retrieves the FileStream content

            Stream fileStream = null;
              
            _contentReadStream.Setup(_configRepository.FileStreamDatabase, fileId);
  
            if (_contentReadStream.Length > 0)
            {
                fileStream = _contentReadStream as Stream;
           
            }
            return fileStream;
        }
    }
}
