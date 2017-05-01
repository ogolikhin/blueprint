using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ImageRenderService.ImageGen
{
    public class SemaphoreQueue
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentQueue<TaskCompletionSource<bool>> _queue;

        public SemaphoreQueue(int initialCount, int maxCount)
        {
            _semaphore = new SemaphoreSlim(initialCount, maxCount);
            _queue = new ConcurrentQueue<TaskCompletionSource<bool>>();
        }

        public bool Wait(int millisecondsTimeout)
        {
            var tcs = new TaskCompletionSource<bool>();
            _queue.Enqueue(tcs);
            _semaphore.WaitAsync(millisecondsTimeout).ContinueWith(t =>
            {
                TaskCompletionSource<bool> poppedTcs;
                if (_queue.TryDequeue(out poppedTcs))
                {
                    poppedTcs.SetResult(true);
                }
            });
            return tcs.Task.Result;
        }

        public void Release(int releaseCount)
        {
            _semaphore.Release(releaseCount);
        }
    }
}