namespace BluePrintSys.Messaging.CrossCutting.Collections
{
    internal class ConcurrentQueue<T>:IQueue<T>
    {
        private readonly IQueue<T> _queue;
        private readonly Semaphore _emptySem;

        public ConcurrentQueue(IQueue<T> queue)
        {
            _queue = queue;
            _emptySem = new Semaphore(0);
        }

        public void Enqueue(T entity)
        {
            lock (_queue)
            {
                _queue.Enqueue(entity);
            }
            _emptySem.Up();
        }

        public T Dequeue()
        {
            _emptySem.Down();
            lock (_queue)
            {
                return _queue.Dequeue();
            }
        }

        public int Count()
        {
            lock (_queue)
            {
                return _queue.Count();
            }
        }
    }
}