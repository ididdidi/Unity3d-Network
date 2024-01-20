using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;
using System.Linq;

namespace ru.ididdidi.Unity3D {
    public class Example : MonoBehaviour
    {
        private string cachedPath = "cache";
        private string materialName = "VideoMaterial";
        private string prefabName = "RickCube";

        [SerializeField] private Image progress;

        /// <summary>
        /// List of loaded asset bundles.
        /// </summary>
        private Stack<AssetBundle> loadedBundles = new Stack<AssetBundle>();
        CancellationTokenSource cancelationToken = new CancellationTokenSource();

        private void Awake()
        {
            // Set the folder for storing cached data
            ResourceCache.ConfiguringCaching(cachedPath);
        }

        // Start is called before the first frame update
        private async void Start()
        {
            // Initialization paths
            var path = Directory.GetDirectories(Application.dataPath, "AssetBundles", SearchOption.AllDirectories).FirstOrDefault();

            if (string.IsNullOrEmpty(path)) { throw new System.Exception("Sample files not found"); }
            path.Replace('\\', '/');

            string materialsURL = $"file://{path}/materials";
            string prefabURL = $"file://{path}/prefabs";
            string movieUrl = $"file://{path.Replace("AssetBundles", "Resources/Video/Rick Astley - Never Gonna Give You Up(18 seconds).mp4")}";

            // Download materials (needed for the prefab)
            await DownloadFromBundle<Material>(materialsURL, materialName);
            
            // Download prefab
            var prefab = await DownloadFromBundle<GameObject>(prefabURL, prefabName);
            
            // Add prefab on the scene
            var videoPlayer = Instantiate(prefab).GetComponent<VideoPlayer>();
            
            // Download video clip
            progress.enabled = true;
            videoPlayer.url = await DownloadVideo(movieUrl);
            progress.enabled = false;
            
            // Let's play the video
            videoPlayer.Play();
        }

        /// <summary>
        /// Loading resources from an Asset Bundle
        /// </summary>
        /// <typeparam name="T">Resource type</typeparam>
        /// <param name="url">Path to the Asset Bundle file (https:// or file://)</param>
        /// <param name="name">Name of the object in the Asset Bundle</param>
        /// <returns></returns>
        private async Task<T> DownloadFromBundle<T>(string url, string name) where T : Object
        {
            var bundle = await Network.GetAssetBundle(url, cancelationToken);
            loadedBundles.Push(bundle);
            return bundle.LoadAsset<T>(name);
        }

        /// <summary>
        /// Loading Video
        /// </summary>
        /// <param name="url">Path to the video file</param>
        /// <returns></returns>
        private async Task<string> DownloadVideo(string url)
        {
            return await Network.GetVideoStream(url, cancelationToken, (prg) => { progress.fillAmount = prg; }, false, true);
        }

        private void OnDestroy()
        {
            // Freeing up resources
            cancelationToken.Cancel();
            while(loadedBundles.Count > 0)
            {
                loadedBundles.Pop().Unload(true);
            }
        }
    }
}