using System.Collections.Generic;

namespace Weave.RssAggregator.HighFrequency
{
    public class ProcessQueue<T>
    {
        readonly PerfQueue<T> queue = new PerfQueue<T>();
        readonly List<T> processList = new List<T>();
        readonly object sync = new object();

        static readonly T defaultForT = default(T);

        public T GetNextFromQueue()
        {
            lock (sync)
            {
                var next = queue.Dequeue();
                if (next == null)
                    return defaultForT;

                processList.Add(next);
                return next;
            }
        }

        public void Enqueue(T o)
        {
            queue.Enqueue(o);
        }

        public void Requeue(T o)
        {
            lock (sync)
            {
                processList.Remove(o);
                queue.Enqueue(o);
            }
        }     
    }




    #region helper class - better version of a Queue<T>

    class PerfQueue<T>
    {
        LinkedList<T> inner = new LinkedList<T>();

        public T Dequeue()
        {
            var first = inner.First;

            if (first != null)
            {
                inner.RemoveFirst();
                return first.Value;
            }

            return default(T);
        }

        public void Enqueue(T val)
        {
            inner.AddLast(val);
        }

        public override string ToString()
        {
            return inner.ToString();
        }
    }

    #endregion
}