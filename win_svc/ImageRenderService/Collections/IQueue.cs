namespace ImageRenderService.Collections
{
    public interface IQueue<T>
    {
        void Enqueue(T entity);
        T Dequeue();
        int Count();
    }
}