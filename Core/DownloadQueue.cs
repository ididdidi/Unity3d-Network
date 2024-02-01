using System;
using System.Collections.Generic;
using UnityEngine;

namespace ru.ididdidi.Unity3D
{
    /// <summary>
    /// 
    /// </summary>
    public class DownloadQueue
    {
        public class Item
        {
            public Hash128 version { get; }
            public IWebRequest request;

            public Item(Hash128 version, IWebRequest request)
            {
                this.version = version;
                this.request = request ?? throw new ArgumentNullException(nameof(request));
            }

            public override bool Equals(object obj) => obj is Item item && version.Equals(item.version);

            public override int GetHashCode()
            {
                var hashCode = -1862655263;
                hashCode = hashCode * -1521134295 + version.GetHashCode();
                return hashCode;
            }
        }

        // Request sequence
        private LinkedList<Item> deque = new LinkedList<Item>();
        private LinkedListNode<Item> hipe;

        public int Count { get => deque.Count; }

        public bool Contains(Item item) => deque.Contains(item);

        public void Add<T>(Item item, bool isCached)
        {
            lock (deque)
            {
                if(hipe == null)
                {
                    hipe = deque.AddFirst(item);
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

        public Item Dequeue()
        {
            lock (deque)
            {
                Item item = hipe?.Value;
                ChangeHipe();
                return item;
            }
        }

        public void Remove(Item item)
        {
            lock (deque)
            {
                var node = deque.Find(item);
                if (node == hipe) { ChangeHipe(); }
                else { deque.Remove(node); }
            }
        }

        private void ChangeHipe()
        {
            var node = hipe;
            if (hipe.Previous != null) { hipe = hipe.Previous; }
            else if (hipe.Next != null) { hipe = hipe.Next; }
            else { hipe = null; }
            deque.Remove(node);
        }

        public void Clear()
        {
            deque.Clear();
        }
    }
}
