﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System.IO;
using System.Linq;

namespace ru.ididdidi.Unity3D {
    public class Example : MonoBehaviour
    {
        private string materialName = "VideoMaterial";
        private string prefabName = "RickCube";

        [SerializeField] private DownloadManager downloadManager;

        /// <summary>
        /// List of loaded asset bundles.
        /// </summary>
        private Stack<AssetBundle> loadedBundles = new Stack<AssetBundle>();

        // Start is called before the first frame update
        private void Start()
        {

            // Initialization paths
            var path = Directory.GetDirectories(Application.dataPath, "AssetBundles", SearchOption.AllDirectories).FirstOrDefault();

            if (string.IsNullOrEmpty(path)) { throw new System.Exception("Sample files not found"); }
            path.Replace('\\', '/');

            string materialsURL = $"file://{path}/materials";
            string prefabURL = $"file://{path}/prefabs";
            string movieUrl = $"file://{path.Replace("AssetBundles", "Resources/Video/Rick Astley - Never Gonna Give You Up(18 seconds).mp4")}";
            
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