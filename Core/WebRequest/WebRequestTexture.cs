using UnityEngine;
using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    public class WebRequestTexture : WebRequest<Texture2D>
    {
        public WebRequestTexture(string url) : base(url) { }

        public override async void Send()
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            
            Texture2D texture = await request.SendWebRequest(
                (response) => DownloadHandlerTexture.GetContent(request), CancelToken, ProgressHandler);
            
            texture.name = System.IO.Path.GetFileNameWithoutExtension(request.url);
            Handler?.Invoke(texture);
            request.Dispose();
        }
    }
}