using UnityEngine;
using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    public class WebRequestAssetBundle : WebRequest<AssetBundle>
    {
        public WebRequestAssetBundle(string url) : base(url) { }

        public override async void Send()
        {
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url);
            Handler?.Invoke(await request.SendWebRequest(
                (response) => DownloadHandlerAssetBundle.GetContent(request), CancelToken, ProgressHandler));
            request.Dispose();
        }
    }
}