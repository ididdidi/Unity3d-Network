using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    public class WebRequestData : WebRequest<byte[]>
    {
        public WebRequestData(string url) : base(url) { }

        protected override async Task<UnityWebRequest> GetWebResponse()
        {
            UnityWebRequest responce = await GetResponse(UnityWebRequest.Get(url), progress);
            if (CacheService.Caching)
            {
                CacheService.SeveToCache(url, await GetVersion(), responce.downloadHandler.data);
            }
            return responce;
        }

        protected override async Task<UnityWebRequest> GetCacheResponse()
        {
            string path = url.ConvertToCachedPath(await GetVersion());
            return await GetResponse(UnityWebRequest.Get(path));
        }

        protected override void HandleResponse(UnityWebRequest response)
        {
            onResponse?.Invoke(response.downloadHandler.data);
        }
    }
}