using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    public class WebRequestTexture : WebRequest<Texture2D>
    {
        public WebRequestTexture(string url) : base(url) { }

        protected override async Task<UnityWebRequest> GetWebResponse()
        {
            UnityWebRequest responce = await GetResponse(UnityWebRequestTexture.GetTexture(url), progress);
            if (CacheService.Caching)
            {
                CacheService.SeveToCache(url, await GetVersion(), responce.downloadHandler.data);
            }
            return responce;
        }

        protected override async Task<UnityWebRequest> GetCacheResponse()
        {
            string path = url.ConvertToCachedPath(await GetVersion());
            return await GetResponse(UnityWebRequestTexture.GetTexture(path));
        }

        protected override void HandleResponse(UnityWebRequest response)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(response);
            texture.name = System.IO.Path.GetFileNameWithoutExtension(response.url);
            onResponse(texture);
        }
    }
}