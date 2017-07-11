namespace BluePrintSys.Messaging.CrossCutting.Collections
{
    public interface IQueue<T>
    {
        void Enqueue(T entity);
        T Dequeue();
        int Count();
    }
}