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

        private const int COMMAND_TIMEOUT = 60;

        /// <summary>
        ///
        /// </summary>
        internal SqlConnection sqlConnection;

        /// <summary>
        ///
        /// </summary>
        internal Guid fileGuid;

        /// <summary>
        ///
        /// </summary>
        internal SqlCommand sqlCommand;

        /// <summary>
        ///
        /// </summary>
        internal SqlParameter pContent;

        /// <summary>
        ///
        /// </summary>
        internal SqlParameter pOffset;

        /// <summary>
        ///
        /// </summary>
        internal SqlParameter pCount;

        /// <summary>
        ///
        /// </summary>
        internal long length = -1L;

        /// <summary>
        ///
        /// </summary>
        internal long position = 0L;

        /// <summary>
        ///
        /// </summary>
        internal ContentReadStream(string connectionString, Guid fileGuid)
            : base()
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }
            sqlConnection = new SqlConnection(connectionString);
            this.fileGuid = fileGuid;
            sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandTimeout = COMMAND_TIMEOUT; 
            sqlCommand.CommandType = CommandType.Text;
            sqlCommand.CommandText = "SELECT @pContent = SUBSTRING([Content], @pOffset, @pCount ) FROM [dbo].[Files] WHERE ([FileGuid] = @pFileGuid);";
            sqlCommand.Parameters.AddWithValue("@pFileGuid", fileGuid);
            pOffset = sqlCommand.Parameters.AddWithValue("@pOffset", 0L);
            pCount = sqlCommand.Parameters.AddWithValue("@pCount", 0L);
            pContent = sqlCommand.Parameters.Add("@pContent", SqlDbType.VarBinary, int.MaxValue);
            pContent.Direction = ParameterDirection.Output;
        }

        /// <summary>
        ///
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return (sqlConnection != null);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return (sqlConnection != null);
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
                if (sqlConnection == null)
                {
                    throw new ObjectDisposedException(string.Empty);
                }
                if (length < 0L)
                {
                    try
                    {
                        if (sqlConnection.State != ConnectionState.Open)
                        {
                            sqlConnection.Open();
                        }
                        SqlCommand sqlCommand = sqlConnection.CreateCommand();
                        sqlCommand.CommandTimeout = COMMAND_TIMEOUT;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = "SELECT @pLength = DATALENGTH([Content]) FROM [dbo].[Files] WHERE ([FileGuid] = @pFileGuid);";
                        sqlCommand.Parameters.AddWithValue("@pFileGuid", fileGuid);
                        SqlParameter pLength = sqlCommand.Parameters.AddWithValue("@pLength", 0L);
                        pLength.Direction = ParameterDirection.Output;
                        sqlCommand.ExecuteNonQuery();
                        length = (pLength.Value is DBNull) ? 0L : (Int64)pLength.Value;
                    }
                    catch (Exception e)
                    {
                        throw new IOException(e.Message, e);
                    }
                }
                return length;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public override long Position
        {
            get
            {
                if (sqlConnection == null)
                {
                    throw new ObjectDisposedException(string.Empty);
                }
                return position;
            }
            set
            {
                long length = Length; // throws ObjectDisposedException
                if ((value < 0L) || (length < value))
                {
                    throw new IOException(string.Empty, new ArgumentOutOfRangeException("value"));
                }
                position = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (sqlConnection == null)
            {
                return;
            }
            try
            {
                if (sqlConnection.State != ConnectionState.Closed)
                {
                    sqlConnection.Close();
                }
            }
            catch (Exception)
            {
            }
            sqlConnection.Dispose();
            sqlConnection = null;
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
                new ArgumentOutOfRangeException("offset");
            }
            if (count < 0)
            {
                new ArgumentOutOfRangeException("count");
            }
            if (buffer.Length < (offset + count))
            {
                new ArgumentException("buffer");
            }
            long length = Length; // throws ObjectDisposedException
            if (count > (length - position))
            {
                count = (int)(length - position);
            }
            if (count > 0)
            {
                try
                {
                    if (sqlConnection.State != ConnectionState.Open)
                    {
                        sqlConnection.Open();
                    }
                    pOffset.Value = position + 1;
                    pCount.Value = (long)count;
                    sqlCommand.ExecuteNonQuery();
                    if (pContent.Value is DBNull)
                    {
                        count = 0;
                    }
                    else
                    {
                        byte[] content = (byte[])pContent.Value;
                        count = content.Length;
                        Buffer.BlockCopy(content, 0, buffer, offset, count);
                        position += count;
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
                    if (sqlConnection.State != ConnectionState.Closed)
                    {
                        sqlConnection.Close();
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
            if (sqlConnection == null)
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
                    position = offset;
                    break;
                case SeekOrigin.Current:
                    if (((position + offset) < 0L) || (length < (position + offset)))
                    {
                        throw new IOException(string.Empty, new ArgumentOutOfRangeException("offset"));
                    }
                    position += offset;
                    break;
                case SeekOrigin.End:
                    if (((length + offset) < 0L) || (length < (length + offset)))
                    {
                        throw new IOException(string.Empty, new ArgumentOutOfRangeException("offset"));
                    }
                    position = length + offset;
                    break;
                default:
                    throw new NotSupportedException();
            }
            return position;
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
