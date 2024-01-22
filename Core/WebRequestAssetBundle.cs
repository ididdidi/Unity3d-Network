using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    public class WebRequestAssetBundle : WebRequest<AssetBundle>
    {
        private CachedAssetBundle assetBundleVersion = default;

        public WebRequestAssetBundle(string url) : base(url) { }

        public WebRequestAssetBundle SetCached(CachedAssetBundle assetBundleVersion)
        {
            this.assetBundleVersion = assetBundleVersion;
            return this;
        }

        public override async Task Send()
        {
            bool isCached = false;
            UnityWebRequest uwr;
            if (string.IsNullOrEmpty(assetBundleVersion.name))
            {
                uwr = UnityWebRequestAssetBundle.GetAssetBundle(url);
            }
            else
            {
                isCached = Caching.IsVersionCached(assetBundleVersion);
                uwr = UnityWebRequestAssetBundle.GetAssetBundle(url, assetBundleVersion);
            }

            UnityWebRequest request = await Send(uwr, isCached ? null : progress);
            if (request != null)
            {
                AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(request);
                response(assetBundle);
            }
        }
    }
}