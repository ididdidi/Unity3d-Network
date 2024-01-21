using System.Collections.Generic;
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

        private string materialName = "VideoMaterial";
        private string prefabName = "RickCube";

        public void DownloadData(string url, System.Action<byte[]> response)
        {
            AddInDownloadQueue(url, response, () => new WebRequestData(url));
        }

        public void DownloadText(string url, System.Action<string> response)
        {
            AddInDownloadQueue(url, response, () => new WebRequestText(url));
        }

        public void DownloadTexture(string url, System.Action<Texture2D> response)
        {
            AddInDownloadQueue(url, response, () => new WebRequestTexture(url));
        }

        public void DownloadAudioClip(string url, System.Action<AudioClip> response)
        {
            AddInDownloadQueue(url, response, () => new WebRequestAudioClip(url));
        }

        public void DownloadVideoClip(string url, System.Action<string> response)
        {
            AddInDownloadQueue<byte[]>(url, (data) => { response(url); }, () => new WebRequestData(url));
        }

        public void DownloadAssetBundle(string url, System.Action<AssetBundle> response)
        {
            AddInDownloadQueue(url, response, () => new WebRequestAssetBundle(url));
        }

        private delegate WebRequest<T> CreateRequest<T>();

        private void AddInDownloadQueue<T>(string url, System.Action<T> response,  CreateRequest<T> createRequest)
        {
            Hash128 id = Hash128.Compute(url);
            if (requests.TryGetValue(id, out IRequest value)) {
                ((WebRequest<T>)value).AddResponse(response);
            }
            else
            {
                downloadQueue.Enqueue(id);
                requests.Add(id, createRequest().AddResponse(response));
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
