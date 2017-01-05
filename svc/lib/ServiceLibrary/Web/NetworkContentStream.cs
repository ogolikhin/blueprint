using System;
using System.IO;

namespace ServiceLibrary.Web
{
    // The default stream System.Net.ConnectStream from File Storage API
    // returns a stream that does not support Seek (Position, Length, Seek()).
    // Since we have access to file content from the HTTP response header we can
    // use this class as a wrapper for the original stream which returns the actual Length
    public class NetworkFileStream : Stream
    {
        private Stream _stream;
        private long _length;

        public NetworkFileStream(Stream stream, long length)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            _stream = stream;
            _length = length;
        }

        public override bool CanRead
        {
            get
            {
                return _stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return _stream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return _stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                if (_stream.CanSeek)
                {
                    return _stream.Length;
                }

                return _length;
            }
        }

        public override long Position
        {
            get
            {
                return _stream.Position;
            }

            set
            {
                _stream.Position = value;
            }
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }
    }
}
