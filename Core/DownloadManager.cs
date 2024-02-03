using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ru.ididdidi.Unity3D
{
    /// <summary>
    /// A class that controls adding a request to the queue and loading it.
    /// </summary>
    public class DownloadManager : MonoBehaviour
    {
        private DownloadQueue.Item current;
        private DownloadQueue downloadQueue = new DownloadQueue();

        [SerializeField] private Image progressView;

        // Start is called before the first frame update
        private void Start() => UnityCacheService.Caching = true;

        /// <summary>
        /// Method for adding download requests.
        /// </summary>
        /// <typeparam name="T">Type of downloaded resource</typeparam>
        /// <param name="request">Web request to download a resource</param>
        public async void Download<T>(WebRequest<T> request)
        {
            Hash128 version = await GetVersion(request);
            bool isCached = UnityCacheService.IsCached(request.url, version);

            if (isCached) { request.url = request.url.GetCachedPath(version); }
            else { request.SetProgressHandler(SetProgress); }

            downloadQueue.Add<T>(new DownloadQueue.Item(version, request), isCached);

            DownloadResources();
        }

        /// <summary>
        /// Method for canceling resource loading.
        /// </summary>
        /// <typeparam name="T">Type of downloaded resource</typeparam>
        /// <param name="request">Web request to download a resource</param>
        public async void Cancel<T>(WebRequest<T> request)
        {
            DownloadQueue.Item download = new DownloadQueue.Item(await GetVersion(request), request);

            if (downloadQueue.Contains(download))
            {
                downloadQueue.Remove<T>(download);
            }

            if (current != null && current.version.Equals(download.version))
            {
                current.request.Cancel();
                current = null;
            }

        }

        private void SetProgress(float progress)
        {
            if (progressView)
            {
                progressView.fillAmount = progress;
                if(progress == 1) { progressView.fillAmount = 0f; }
            }
        }

        /// <summary>
        /// Method for getting the latest available version of a downloadable resource.
        /// </summary>
        /// <typeparam name="T">Web request to download a resource</typeparam>
        /// <param name="request">Web request to download a resource</param>
        /// <returns></returns>
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

        /// <summary>
        /// Method for downloading a resource.
        /// </summary>
        private async void DownloadResources()
        {
            if (current != null) { return; }

            while (downloadQueue.Count > 0)
            {
                current = downloadQueue.Dequeue();
                if (UnityCacheService.Caching && !current.request.url.Contains(Application.persistentDataPath))
                {
                    try
                    {
                        byte[] data = await UnityNetService.GetData(current.request.url, current.request.CancelToken, SetProgress);
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

        /// <summary>
        /// Method to release resources and cancel downloads.
        /// </summary>
        private void OnDestroy()
        {
            downloadQueue?.Clear();
            current?.request.Cancel();
        }
    }
}
