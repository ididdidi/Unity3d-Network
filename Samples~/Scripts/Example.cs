using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace ru.ididdidi.Unity3D {
    public class Example : MonoBehaviour
    {
        private string materialName = "VideoMaterial";
        private string prefabName = "RickCube";

        [SerializeField] private DownloadManager downloadManager;

        [SerializeField] private string materialsURL = "https://raw.githubusercontent.com/ididdidi/Unity3d-Network/test/Samples~/AssetBundles/materials";
        [SerializeField] private string prefabURL = "https://raw.githubusercontent.com/ididdidi/Unity3d-Network/test/Samples~/AssetBundles/prefabs";
        [SerializeField] private string movieUrl = "https://media.githubusercontent.com/media/ididdidi/Unity3d-Network/test/Samples~/Resources/Video/" +
            "Rick%20Astley%20-%20Never%20Gonna%20Give%20You%20Up(18%20seconds).mp4";

        /// <summary>
        /// List of loaded asset bundles.
        /// </summary>
        private Stack<AssetBundle> loadedBundles = new Stack<AssetBundle>();

        // Start is called before the first frame update
        private void Start()
        {            
            // Download materials (needed for the prefab)
            downloadManager.Download(new WebRequestAssetBundle(materialsURL).AddHandler((bundle) =>
            {
                loadedBundles.Push(bundle);
                bundle.LoadAsset<Material>(materialName);
            }));

            // Download prefab
            downloadManager.Download(new WebRequestAssetBundle(prefabURL).AddHandler((bundle) =>
            {
                 loadedBundles.Push(bundle);
                 var videoPlayer = Instantiate(bundle.LoadAsset<GameObject>(prefabName)).GetComponent<VideoPlayer>();

                 downloadManager.Download(new WebRequestVideoStream(movieUrl).AddHandler((streamURL) =>
                 {
                     videoPlayer.url = streamURL;
                     videoPlayer.source = VideoSource.Url;
                     videoPlayer.Play();
                 }));
            }));
        }

        private void OnDestroy()
        {
            // Freeing up resources
            while(loadedBundles.Count > 0)
            {
                loadedBundles.Pop().Unload(true);
            }
        }
    }
}