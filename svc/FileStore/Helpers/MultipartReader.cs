using System;
using System.IO;
using System.Threading.Tasks;

namespace FileStore.Helpers
{
    public abstract class MultipartReader: IDisposable
    {
        protected MultipartPartParser MultipartPartParser;

        protected MultipartReader(Stream stream)
        {
            MultipartPartParser = new MultipartPartParser(stream);
        }
        public async Task ReadAndExecuteRequestAsync()
        {
            if (MultipartPartParser.IsEndPart)
            {
                HandleMultipartReadError("End part detected after reading Headers. No more parts detected.");
            }
            if (!MultipartPartParser.IsEndPart && !string.IsNullOrWhiteSpace(MultipartPartParser.Filename))
            {
                await ExecuteFunctionAsync(MultipartPartParser);

                //move the stream foward until we get to the next part
                MultipartPartParser = MultipartPartParser.ReadUntilNextPart();
                if (MultipartPartParser != null)
                {
                    HandleMultipartReadError("Only one part is supported for multi-part.");
                }
                return;
            }
            HandleMultipartReadError("Error reading multi-part");
        }

        protected abstract void HandleMultipartReadError(string error);
        protected abstract Task ExecuteFunctionAsync(Stream stream);

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
                if (MultipartPartParser != null)
                {
                    MultipartPartParser.Dispose();
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