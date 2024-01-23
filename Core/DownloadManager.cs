using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ru.ididdidi.Unity3D
{
    public class DownloadManager : MonoBehaviour
    {
        private string cachedPath = "cache";
        private Queue<Hash128> downloadQueue = new Queue<Hash128>();
        private Dictionary<Hash128, IRequest> requests = new Dictionary<Hash128, IRequest>();
        private IRequest current;

        private void Awake()
        {
            // Set the folder for storing cached data
            ResourceCache.ConfiguringCaching(cachedPath);
        }

        public async void Download<T>(WebRequest<T> request)
        {
            if(!await request.TryGetFromCache())
            {
                AddInDownloadQueue(request);
            }
        }

        private void AddInDownloadQueue<T>(WebRequest<T> request)
        {
            Hash128 id = Hash128.Compute(request.url);
            if (requests.TryGetValue(id, out IRequest value)) {
                ((WebRequest<T>)value).AddResponse(request.Response);
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
