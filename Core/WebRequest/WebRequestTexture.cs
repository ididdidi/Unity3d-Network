using UnityEngine;
using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    /// <summary>
    /// Specialized class for downloading <see cref="Texture2D"/>
    /// </summary>
    public class WebRequestTexture : WebRequest<Texture2D>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="url">URL address</param>
        public WebRequestTexture(string url) : base(url) { }

        /// <summary>
        /// Send a web request to the specified url.
        /// </summary>
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