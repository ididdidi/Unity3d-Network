using System.Collections.Generic;
using UnityEngine;

namespace ru.ididdidi.Unity3D
{
    public class DownloadManager : MonoBehaviour
    {
        private Queue<Hash128> downloadQueue = new Queue<Hash128>();
        private Dictionary<Hash128, IRequest> requests = new Dictionary<Hash128, IRequest>();
        private IRequest current;

        private void Start()
        {
            CacheService.Caching = true;
        }

        public async void Download<T>(WebRequest<T> request)
        {
            if (await request.IsCached()) { await request.Send(); }
            else { AddInDownloadQueue(await request.GetVersion(), request); }
        }

        private void AddInDownloadQueue<T>(Hash128 id, WebRequest<T> request)
        {
            if (requests.TryGetValue(id, out IRequest value)) {
                ((WebRequest<T>)value).AddResponseHandler(request.OnResponse);
            }
            else
            {
                downloadQueue.Enqueue(id);
                requests.Add(id, request);
            }

            DownloadResources();
        }

        private async void DownloadResources()
        {
            if (current != null) { return; }
            
            while (downloadQueue.Count > 0)
            {
                Hash128 id = downloadQueue.Dequeue();
                current = requests[id];
                await current.Send();
                requests.Remove(id);
            }

            current = null;
        }
    }
}
