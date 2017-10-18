using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrintSys.Messaging.CrossCutting.Collections
{
    public interface IHavePriority
    {
        Priority GetPriority();
    }

    public enum Priority
    {
        LOW,
        NORMAL,
        HIGH
    }

    public class PriorityQueue<T> :IQueue<T> where T : IHavePriority
    {
        private readonly Dictionary<Priority, Queue<T>> _priQueue;
        private readonly Priority[] orderedPriorities = { Priority.HIGH, Priority.NORMAL, Priority.LOW };
        private int _count;

        public PriorityQueue()
        {
            _priQueue = new Dictionary<Priority, Queue<T>>();
            foreach (var priority in orderedPriorities)
            {
                _priQueue.Add(priority, new Queue<T>());
            }
            _count = 0;
        }

        public int Count()
        {
            return _count;
        }

        public void Enqueue(T entity)
        {
            _priQueue[entity.GetPriority()].Enqueue(entity);
            ++_count;
        }

        public T Dequeue()
        {
            foreach (var priority in orderedPriorities)
            {
                var queue = _priQueue[priority];
                if (queue.Any())
                {
                    T entity = queue.Dequeue();
                    --_count;
                    return entity;
                }
            }
            throw new InvalidOperationException("Priority Queue Dequeing From Empty");
        }
    }
}