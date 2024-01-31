using System.Collections.Generic;
using UnityEngine;

namespace ru.ididdidi.Unity3D
{
    /// <summary>
    /// 
    /// </summary>
    public class DownloadQueue
    {
        // Request sequence
        private LinkedList<Hash128> deque = new LinkedList<Hash128>();
        private Dictionary<Hash128, IWebRequest> requests = new Dictionary<Hash128, IWebRequest>();

        public int Count { get => deque.Count; }

        public bool Contains(Hash128 id) => deque.Contains(id);

        public void Add<T>(Hash128 id, WebRequest<T> request, bool isCached)
        {
            lock (deque)
            {
                if (deque.Contains(id) && requests.TryGetValue(id, out IWebRequest value))
                {
                    ((WebRequest<T>)value).AddHandler(request.Handler);
                }
                else
                {
                    if (isCached) { deque.AddFirst(id); }
                    else { deque.AddLast(id); }

                    requests.Add(id, request);
                }
            }
        }

        public IWebRequest Dequeue(out Hash128 id)
        {
            lock (deque)
            {
                id = deque.First.Value;
                IWebRequest request = requests[id];
                requests.Remove(id);
                deque.RemoveFirst();
                return request;
            }
        }

        public bool Remove(Hash128 id)
        {
            lock (deque)
            {
                requests.Remove(id);
                return deque.Remove(id);
            }
        }

        public void Clear()
        {
            deque.Clear();
            requests.Clear();
        }
    }
}
