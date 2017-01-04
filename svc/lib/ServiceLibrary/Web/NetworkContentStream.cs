using System;
using System.IO;

namespace ServiceLibrary.Web
{
    public class NetworkFileStream : Stream
    {
        private Stream stream;
        private long length;

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

            this.stream = stream;
            this.length = length;
        }

        public override bool CanRead
        {
            get
            {
                return stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return stream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                if (stream.CanSeek)
                {
                    return stream.Length;
                }

                return length;
            }
        }

        public override long Position
        {
            get
            {
                return stream.Position;
            }

            set
            {
                stream.Position = value;
            }
        }

        public override void Flush()
        {
            stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
        }
    }
}
