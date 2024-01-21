using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    public class WebRequestTexture : WebRequest<Texture2D>
    {
        public WebRequestTexture(string url) : base(url) { }

        public override async Task Send()
        {
            UnityWebRequest request = await Send(UnityWebRequestTexture.GetTexture(url), progress);
            if (request != null)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                texture.name = System.IO.Path.GetFileNameWithoutExtension(request.url);
                response(texture);
            }
        }
    }
}