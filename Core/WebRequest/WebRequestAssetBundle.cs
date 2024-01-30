using UnityEngine;
using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    /// <summary>
    /// Specialized class for downloading <see cref="AssetBundle"/>
    /// </summary>
    public class WebRequestAssetBundle : WebRequest<AssetBundle>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="url">URL address</param>
        public WebRequestAssetBundle(string url) : base(url) { }

        /// <summary>
        /// Send a web request to the specified url.
        /// </summary>
        public override async void Send()
        {
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url);
            Handler?.Invoke(await request.SendWebRequest(
                (response) => DownloadHandlerAssetBundle.GetContent(request), CancelToken, ProgressHandler));
            request.Dispose();
        }
    }
}