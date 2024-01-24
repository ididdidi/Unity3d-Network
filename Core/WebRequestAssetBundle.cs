using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    public class WebRequestAssetBundle : WebRequest<AssetBundle>
    {
        public WebRequestAssetBundle(string url) : base(url) { }

        protected override async Task<Hash128> GetLatestVersion()
        {
            await new WebRequestText(url + ".manifest").AddResponseHandler((str) => {
                var hashRow = str.Split("\n".ToCharArray())[5];
                hash = Hash128.Parse(hashRow.Split(':')[1].Trim());
            }).Send();
            return hash;
        }

        private async Task<CachedAssetBundle> GetCachedAssetBundle() => new CachedAssetBundle(Hash128.Compute(url).ToString(), await GetVersion());

        public override async Task<bool> IsCached() => Caching.IsVersionCached(await GetCachedAssetBundle());

        protected override async Task<UnityWebRequest> GetWebResponse()
        {
            return await GetResponse(UnityWebRequestAssetBundle.GetAssetBundle(url, await GetCachedAssetBundle()), progress);
        }

        protected async override Task<UnityWebRequest> GetCacheResponse()
        {
            return await GetResponse(UnityWebRequestAssetBundle.GetAssetBundle(url, await GetCachedAssetBundle()));
        }

        protected override void HandleResponse(UnityWebRequest response)
        {
            onResponse?.Invoke(DownloadHandlerAssetBundle.GetContent(response));
        }
    }
}