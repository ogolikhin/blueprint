using System;
using System.IO;
using System.Threading.Tasks;

namespace FileStore.Helpers
{
    public abstract class MultipartReader: IDisposable
    {
        protected MultipartPartParser MultiPartParser;

        protected MultipartReader(Stream stream)
        {
            MultiPartParser = new MultipartPartParser(stream);
        }
        public async Task ReadAndExecuteRequest()
        {
            if (MultiPartParser.IsEndPart)
            {
                HandleMultipartReadError("End part detected after reading Headers. No more parts detected.");
            }
            if (!MultiPartParser.IsEndPart && !string.IsNullOrWhiteSpace(MultiPartParser.Filename))
            {
                await ExeucteFunction(MultiPartParser);

                //move the stream foward until we get to the next part
                MultiPartParser = MultiPartParser.ReadUntilNextPart();
                if (MultiPartParser != null)
                {
                    HandleMultipartReadError("Only one part is supported for multi-part.");
                }
                return;
            }
            HandleMultipartReadError("Error reading multi-part");
        }

        protected abstract void HandleMultipartReadError(string error);
        protected abstract Task ExeucteFunction(Stream stream);

        #region Dispose Methods
        bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
                if (MultiPartParser != null)
                {
                    MultiPartParser.Dispose();
                }
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }
        #endregion Dispose Methods
    }
    [Serializable]
    public class MultipartReadException : Exception
    {
        public MultipartReadException(string message)
            : base(message)
        {
        }
    }
}