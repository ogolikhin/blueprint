//------------------------------------------------------------------------------------------------
// Author(s)         : Alexander Groman
// Creation Date     : 08/30/2011
// Short description : Stream implementation to provide Transact-SQL Read Access to FILESTREAM content
//------------------------------------------------------------------------------------------------

using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace FileStore.Repositories
{

    /// <summary>
    ///
    /// </summary>
    public class ContentReadStream : Stream
    {
        // The time in seconds to wait for the command to execute. The default is 30 seconds.

        private const int CommandTimeout = 60;

        /// <summary>
        ///
        /// </summary>
        private SqlConnection _sqlConnection;

        /// <summary>
        ///
        /// </summary>
        private readonly Guid _fileGuid;

        /// <summary>
        ///
        /// </summary>
        private readonly SqlCommand _sqlCommand;

        /// <summary>
        ///
        /// </summary>
        private readonly SqlParameter _pContent;

        /// <summary>
        ///
        /// </summary>
        private readonly SqlParameter _pOffset;

        /// <summary>
        ///
        /// </summary>
        private readonly SqlParameter _pCount;

        /// <summary>
        ///
        /// </summary>
        private long _length = -1L;

        /// <summary>
        ///
        /// </summary>
        private long _position;

        /// <summary>
        /// 
        /// </summary>
        private string _fileType;

        /// <summary>
        /// 
        /// </summary>
        private string _fileName;

        /// <summary>
        ///
        /// </summary>
        internal ContentReadStream(string connectionString, Guid fileGuid)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }
            _sqlConnection = new SqlConnection(connectionString);
            _fileGuid = fileGuid;
            _sqlCommand = _sqlConnection.CreateCommand();
            _sqlCommand.CommandTimeout = CommandTimeout; 
            _sqlCommand.CommandType = CommandType.Text;
            _sqlCommand.CommandText = "SELECT @pContent = SUBSTRING([Content], @pOffset, @pCount ) FROM [dbo].[Files] WHERE ([FileGuid] = @pFileGuid);";
            _sqlCommand.Parameters.AddWithValue("@pFileGuid", fileGuid);
            _pOffset = _sqlCommand.Parameters.AddWithValue("@pOffset", 0L);
            _pCount = _sqlCommand.Parameters.AddWithValue("@pCount", 0L);
            _pContent = _sqlCommand.Parameters.Add("@pContent", SqlDbType.VarBinary, int.MaxValue);
            _pContent.Direction = ParameterDirection.Output;
        }

        /// <summary>
        ///
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return (_sqlConnection != null);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return (_sqlConnection != null);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public override long Length
        {
            get
            {
                if (_sqlConnection == null)
                {
                    throw new ObjectDisposedException(string.Empty);
                }
                if (_length < 0L)
                {
                    try
                    {
                        if (_sqlConnection.State != ConnectionState.Open)
                        {
                            _sqlConnection.Open();
                        }
                        SqlCommand sqlCommand = _sqlConnection.CreateCommand();
                        sqlCommand.CommandTimeout = CommandTimeout;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = "SELECT @pLength = DATALENGTH([Content]) FROM [dbo].[Files] WHERE ([FileGuid] = @pFileGuid);";
                        sqlCommand.Parameters.AddWithValue("@pFileGuid", _fileGuid);
                        SqlParameter pLength = sqlCommand.Parameters.AddWithValue("@pLength", 0L);
                        pLength.Direction = ParameterDirection.Output;
                        sqlCommand.ExecuteNonQuery();
                        _length = (pLength.Value is DBNull) ? 0L : (Int64)pLength.Value;
                    }
                    catch (Exception e)
                    {
                        throw new IOException(e.Message, e);
                    }
                }
                return _length;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string FileType
        {
            get
            {
                if (_sqlConnection == null)
                {
                    throw new ObjectDisposedException(string.Empty);
                }
                if (_fileType == null)
                {
                    try
                    {
                        if (_sqlConnection.State != ConnectionState.Open)
                        {
                            _sqlConnection.Open();
                        }
                        SqlCommand sqlCommand = _sqlConnection.CreateCommand();
                        sqlCommand.CommandTimeout = CommandTimeout;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = "SELECT TOP 1 @pType = [Type] FROM [dbo].[AttachmentVersions] WHERE ([File_FileGuid] = @pFileGuid);";
                        sqlCommand.Parameters.AddWithValue("@pFileGuid", _fileGuid);
                        SqlParameter pType = sqlCommand.Parameters.Add("@pType", SqlDbType.NVarChar, Int32.MaxValue);
                        pType.Direction = ParameterDirection.Output;
                        sqlCommand.ExecuteNonQuery();
                        _fileType = (pType.Value is DBNull) ? string.Empty : (string)pType.Value;
                    }
                    catch (Exception e)
                    {
                        throw new IOException(e.Message, e);
                    }
                }
                return _fileType;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string FileName
        {
            get
            {
                if (_sqlConnection == null)
                {
                    throw new ObjectDisposedException(string.Empty);
                }
                if (_fileName == null)
                {
                    try
                    {
                        if (_sqlConnection.State != ConnectionState.Open)
                        {
                            _sqlConnection.Open();
                        }
                        SqlCommand sqlCommand = _sqlConnection.CreateCommand();
                        sqlCommand.CommandTimeout = CommandTimeout;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = "SELECT TOP 1 @pName = [Name] FROM [dbo].[AttachmentVersions] WHERE ([File_FileGuid] = @pFileGuid);";
                        sqlCommand.Parameters.AddWithValue("@pFileGuid", _fileGuid);
                        SqlParameter pName = sqlCommand.Parameters.Add("@pName", SqlDbType.NVarChar, Int32.MaxValue);
                        pName.Direction = ParameterDirection.Output;
                        sqlCommand.ExecuteNonQuery();
                        _fileName = (pName.Value is DBNull) ? string.Empty : (string)pName.Value;
                    }
                    catch (Exception e)
                    {
                        throw new IOException(e.Message, e);
                    }
                }
                return _fileName;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public override long Position
        {
            get
            {
                if (_sqlConnection == null)
                {
                    throw new ObjectDisposedException(string.Empty);
                }
                return _position;
            }
            set
            {
                long length = Length; // throws ObjectDisposedException
                if ((value < 0L) || (length < value))
                {
                    throw new IOException(string.Empty, new ArgumentOutOfRangeException("value"));
                }
                _position = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (_sqlConnection == null)
            {
                return;
            }
            try
            {
                if (_sqlConnection.State != ConnectionState.Closed)
                {
                    _sqlConnection.Close();
                }
            }
            catch
            {
                // ignored
            }
            _sqlConnection.Dispose();
            _sqlConnection = null;
        }

        /// <summary>
        ///
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        ///
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (buffer.Length < (offset + count))
            {
                throw new ArgumentException("buffer");
            }
            long length = Length; // throws ObjectDisposedException
            if (count > (length - _position))
            {
                count = (int)(length - _position);
            }
            if (count > 0)
            {
                try
                {
                    if (_sqlConnection.State != ConnectionState.Open)
                    {
                        _sqlConnection.Open();
                    }
                    _pOffset.Value = _position + 1;
                    _pCount.Value = (long)count;
                    _sqlCommand.ExecuteNonQuery();
                    if (_pContent.Value is DBNull)
                    {
                        count = 0;
                    }
                    else
                    {
                        byte[] content = (byte[])_pContent.Value;
                        count = content.Length;
                        Buffer.BlockCopy(content, 0, buffer, offset, count);
                        _position += count;
                    }
                }
                catch (Exception e)
                {
                    throw new IOException(e.Message, e);
                }
            }
            if (count <= 0)
            {
                try
                {
                    if (_sqlConnection.State != ConnectionState.Closed)
                    {
                        _sqlConnection.Close();
                    }
                }
                catch (Exception e)
                {
                    throw new IOException(e.Message, e);
                }
            }
            return count;
        }

        /// <summary>
        ///
        /// </summary>
        public override int ReadByte()
        {
            if (_sqlConnection == null)
            {
                throw new ObjectDisposedException(string.Empty);
            }
            return base.ReadByte();
        }

        /// <summary>
        ///
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            long length = Length; // throws ObjectDisposedException
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if ((offset < 0L) || (length < offset))
                    {
                        throw new IOException(string.Empty, new ArgumentOutOfRangeException("offset"));
                    }
                    _position = offset;
                    break;
                case SeekOrigin.Current:
                    if (((_position + offset) < 0L) || (length < (_position + offset)))
                    {
                        throw new IOException(string.Empty, new ArgumentOutOfRangeException("offset"));
                    }
                    _position += offset;
                    break;
                case SeekOrigin.End:
                    if (((length + offset) < 0L) || (length < (length + offset)))
                    {
                        throw new IOException(string.Empty, new ArgumentOutOfRangeException("offset"));
                    }
                    _position = length + offset;
                    break;
                default:
                    throw new NotSupportedException();
            }
            return _position;
        }

        /// <summary>
        ///
        /// </summary>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///
        /// </summary>
        public override void WriteByte(byte value)
        {
            throw new NotSupportedException();
        }
    }
}
