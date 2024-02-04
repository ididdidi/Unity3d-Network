using System.Collections.Generic;
using UnityEngine;

namespace UnityNetwork
{
    /// <summary>
    /// Priority queue for loading resources.
    /// The queue has two priorities: high and low.
    /// They are determined by the isCached flag. Priority is given to cached resources.
    /// </summary>
    public class DownloadQueue
    {
        /// <summary>
        /// Class that encapsulates the version of the downloaded file and the request for it.
        /// </summary>
        public class Item
        {
            public Hash128 version { get; }
            public IWebRequest request { get; }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="version"></param>
            /// <param name="request"></param>
            public Item(Hash128 version, IWebRequest request)
            {
                this.version = version;
                this.request = request ?? throw new System.ArgumentNullException(nameof(request));
            }

            /// <summary>
            /// Method for comparison.
            /// </summary>
            /// <param name="obj">The object this instance is compared to</param>
            /// <returns>Is equals</returns>
            public override bool Equals(object obj) => obj is Item item && version.Equals(item.version);

            /// <summary>
            /// Method for comparison.
            /// </summary>
            /// <returns>Unity hash</returns>
            public override int GetHashCode()
            {
                var hashCode = -1862655263;
                hashCode = hashCode * -1521134295 + version.GetHashCode();
                return hashCode;
            }
        }

        // Request sequence
        private LinkedList<Item> deque = new LinkedList<Item>();
        private LinkedListNode<Item> heap;

        public int Count { get => deque.Count; }

        /// <summary>
        /// Checks if there is a similar request (with the same version) in the queue.
        /// </summary>
        /// <param name="item">Queue element</param>
        /// <returns>Is contains</returns>
        public bool Contains(Item item) => deque.Contains(item);

        /// <summary>
        /// Add request to queue.
        /// </summary>
        /// <typeparam name="T">Type of downloaded resource</typeparam>
        /// <param name="item">Queue element</param>
        /// <param name="isCached">Is the item cached in device memory?</param>
        public void Add<T>(Item item, bool isCached)
        {
            lock (deque)
            {
                if(heap == null)
                {
                    heap = deque.AddFirst(item);
                }
                else if (deque.Contains(item))
                {
                    ((WebRequest<T>)deque.Find(item).Value.request).AddHandler(((WebRequest<T>)item.request).Handler);
                }
                else
                {
                    if (isCached) { deque.AddFirst(item); }
                    else { deque.AddLast(item); }
                }
            }
        }

        /// <summary>
        /// Retrieves an element from the queue.
        /// </summary>
        /// <returns>Queue element</returns>
        public Item Dequeue()
        {
            if(heap == null) { throw new System.Exception("The top of the queue points to null. " +
                "Add a request to the queue before accessing it or check if there is at least one item in it."); }

            lock (deque)
            {
                Item item = heap?.Value;
                ChangeHipe();
                return item;
            }
        }

        /// <summary>
        /// Removes an element from the queue.
        /// </summary>
        /// <param name="item">Queue element</param>
        public void Remove<T>(Item item)
        {
            lock (deque)
            {
                var node = deque.Find(item);
                var request = (WebRequest<T>)node.Value.request;
                request.RemoveHandler(((WebRequest<T>)item.request).Handler);

                if(request.Handler == null)
                {
                    if (node == heap) { ChangeHipe(); }
                    else { deque.Remove(node); }
                }
            }
        }

        /// <summary>
        /// Move Hipe
        /// </summary>
        private void ChangeHipe()
        {
            var node = heap;
            if (heap.Previous != null) { heap = heap.Previous; }
            else if (heap.Next != null) { heap = heap.Next; }
            else { heap = null; }
            deque.Remove(node);
        }

        /// <summary>
        /// Removes all elements from the queue and heap
        /// </summary>
        public void Clear()
        {
            deque.Clear();
            heap?.Value.request.Cancel();
            heap = null;
        }
    }
}
