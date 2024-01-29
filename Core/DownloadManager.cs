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
            UnityCacheService.Caching = true;
        }

        public async void Download<T>(WebRequest<T> request)
        {
            Hash128 version = await GetVersion(request);
            if (UnityCacheService.IsCached(request.url, version)) {
                GetFromCache(request, version);
            }
            else { AddInDownloadQueue(version, request); }
        }

        private void GetFromCache(IWebRequest request, Hash128 version)
        {
            request.url = request.url.GetCachedPath(version);
            request.Send();
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
                version = UnityCacheService.GetCachedVersion(request.url);
            }
            return version;
        }

        private void AddInDownloadQueue<T>(Hash128 id, WebRequest<T> request)
        {
            if (requests.TryGetValue(id, out IWebRequest value)) {
                ((WebRequest<T>)value).AddHandler(request.Handler);
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
                if (UnityCacheService.Caching)
                {
                    try
                    {
                        byte[] data = await UnityNetService.GetData(current.url);
                        UnityCacheService.SeveToCache(current.url, id, data);
                        GetFromCache(current, id);
                    }
                    catch (System.Exception eror) 
                    {
                        Debug.LogError(eror);
                    }
                }
                else { current.Send(); }
                requests.Remove(id);
            }

            current = null;
        }

        private void OnDestroy()
        {
            downloadQueue?.Clear();
            requests?.Clear();
            current?.Cancel();
        }
    }
}
