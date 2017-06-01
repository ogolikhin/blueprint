using System.Threading;

namespace ImageRenderService.Collections
{
    public class Semaphore
    {
        public Semaphore(int initialCount)
        {
            _count = initialCount;
            _threadsWaiting = 0;
            _monitorObj = new object();
        }

        public void Down()
        {
            lock (_monitorObj)
            {
                P();
            }
        }

        public void Up()
        {
            lock (_monitorObj)
            {
                V();
            }
        }

        private int _count;
        private int _threadsWaiting;
        private readonly object _monitorObj;

        private void P()
        {
            if (_count == 0)
            {
                _threadsWaiting++;
                Monitor.Wait(_monitorObj);
            }
            else
            {
                _count--;
            }
        }

        private void V()
        {
            if (_threadsWaiting > 0)
            {
                _threadsWaiting--;
                Monitor.Pulse(_monitorObj);
            }
            else
            {
                _count++;
            }
        }
    }
}
