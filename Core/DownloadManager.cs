using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ru.ididdidi.Unity3D
{
    public class DownloadManager : MonoBehaviour
    {
        private Queue<Hash128> downloadQueue = new Queue<Hash128>();
        private Dictionary<Hash128, IWebRequest> requests = new Dictionary<Hash128, IWebRequest>();
        private IWebRequest current;

        private void Start()
        {
            CacheService.Caching = true;
        }

        public async void Download<T>(WebRequest<T> request)
        {
            Hash128 version = await GetVersion(request);
            if (CacheService.IsCached(request.url, version)) {
                request.GetFromCache(version);
            }
            else { AddInDownloadQueue(version, request); }
        }

        public async Task<Hash128> GetVersion<T>(WebRequest<T> request)
        {
            Hash128 version = default;
            try
            {
                version = await request.GetLatestVersion();
            }
            catch
            {
                version = CacheService.GetCachedVersion(request.url);
            }
            return version; // Реализовать исключение
        }

        private void AddInDownloadQueue<T>(Hash128 id, WebRequest<T> request)
        {
            if (requests.TryGetValue(id, out IWebRequest value)) {
                ((WebRequest<T>)value).AddResponseHandler(request.Handler);
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
                if (CacheService.Caching)
                {
                    CacheService.SeveToCache(current.url, id, await current.GetData());
                    current.GetFromCache(id);
                }
                else { current.Send(); }
                requests.Remove(id);
            }

            current = null;
        }
    }
}
