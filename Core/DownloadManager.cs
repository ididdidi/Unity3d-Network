using System.Threading.Tasks;
using UnityEngine;

namespace ru.ididdidi.Unity3D
{
    public class DownloadManager : MonoBehaviour
    {
        private class Downloading
        {
            public Hash128 version;
            public IWebRequest request;
        }

        private Downloading current;
        private DownloadQueue downloadQueue = new DownloadQueue();

        private void Start()
        {
            UnityCacheService.Caching = true;
        }

        public async void Download<T>(WebRequest<T> request)
        {
            Hash128 version = await GetVersion(request);
            bool isCached = UnityCacheService.IsCached(request.url, version);

            if (isCached) { request.url = request.url.GetCachedPath(version); }

            downloadQueue.Add(version, request, isCached);

            DownloadResources();
        }

        public async void Cancel<T>(WebRequest<T> request)
        {
            Hash128 version = await GetVersion(request);

            if (downloadQueue.Contains(version))
            {
                downloadQueue.Remove(version);
            }

            if (current != null && current.version.Equals(version))
            {
                current.request.Cancel();
                current = null;
            }

        }

        private async Task<Hash128> GetVersion<T>(WebRequest<T> request)
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

        private async void DownloadResources()
        {
            if (current != null) { return; }

            current = new Downloading();

            while (downloadQueue.Count > 0)
            {
                current.request = downloadQueue.Dequeue(out current.version);
                if (UnityCacheService.Caching)
                {
                    try
                    {
                        byte[] data = await UnityNetService.GetData(current.request.url);
                        UnityCacheService.SeveToCache(current.request.url, current.version, data);
                        current.request.url = current.request.url.GetCachedPath(current.version);
                    }
                    catch (System.Exception eror) 
                    {
                        Debug.LogError(eror);
                    }
                }
                current.request.Send();
            }

            current = null;
        }

        private void OnDestroy()
        {
            downloadQueue?.Clear();
            current?.request.Cancel();
        }
    }
}
