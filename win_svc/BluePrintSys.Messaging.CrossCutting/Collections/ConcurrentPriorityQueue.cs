namespace BluePrintSys.Messaging.CrossCutting.Collections
{
    public class ConcurrentPriorityQueue<T> :IQueue<T> where T : IHavePriority
    {
        private readonly IQueue<T> _priQueue;

        public ConcurrentPriorityQueue()
        {
            _priQueue = new ConcurrentQueue<T>(new PriorityQueue<T>());
        }

        public void Enqueue(T entity)
        {
            _priQueue.Enqueue(entity);
        }

        public T Dequeue()
        {
            return _priQueue.Dequeue();
        }

        public int Count()
        {
            return _priQueue.Count();
        }
    }
}