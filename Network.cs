using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ru.mofrison.Unity3d
{
    public static class Network
    {
        private static async Task<UnityWebRequest> SendWebRequest(UnityWebRequest request, CancellationTokenSource cancelationToken = null, System.Action<float> progress = null)
        {
            while (!Caching.ready)
            {
                if (cancelationToken != null && cancelationToken.IsCancellationRequested)
                {
                    return null;
                }
                await Task.Yield();
            }

#pragma warning disable CS4014
            request.SendWebRequest();
#pragma warning restore CS4014

            while (!request.isDone)
            {
                if (cancelationToken != null && cancelationToken.IsCancellationRequested)
                {
                    request.Abort();
                    request.Dispose();

                    return null;
                }
                else
                {
                    progress?.Invoke(request.downloadProgress);
                    await Task.Yield();
                }
            }
            progress?.Invoke(1f);
            return request;
        }

        public static async Task<long> GetSize(string url)
        {
            UnityWebRequest request = await SendWebRequest(UnityWebRequest.Head(url));
            var contentLength = request.GetResponseHeader("Content-Length");
            if (long.TryParse(contentLength, out long returnValue))
            {
                return returnValue;
            }
            else
            {
                throw new Exception("[Netowrk] error: " + request.error + " " + url);
            }
        }

        public static async Task<string> GetText(string url)
        {
            var uwr = await SendWebRequest(UnityWebRequest.Get(url));
            if (uwr != null && !uwr.isHttpError && !uwr.isNetworkError)
            {
                return uwr.downloadHandler.text;
            }
            else
            {
                throw new Exception("[Netowrk] error: " + uwr.error + " " + uwr.url);
            }
        }

        public static async Task<byte[]> GetData(string url, CancellationTokenSource cancelationToken, System.Action<float> progress = null)
        {
            UnityWebRequest uwr = await SendWebRequest(UnityWebRequest.Get(url), cancelationToken, progress);
            if (uwr != null && !uwr.isHttpError && !uwr.isNetworkError)
            {
                return uwr.downloadHandler.data;
            }
            else
            {
                throw new Exception("[Netowrk] error: " + uwr.error + " " + uwr.uri);
            }
        }

        public static async Task<Texture2D> GetTexture(string url, CancellationTokenSource cancelationToken, System.Action<float> progress = null, bool caching = true)
        {
            string path = await url.GetPathOrUrl();
            bool isCached = path.Contains("file://");
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(path);

            UnityWebRequest uwr = await SendWebRequest(request, cancelationToken, isCached? null : progress);
            if (uwr != null && !uwr.isHttpError && !uwr.isNetworkError)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                texture.name = Path.GetFileName(uwr.url);
                if (caching && !isCached) 
                {
                    try
                    {
                        ResourceCache.Caching(uwr.url, uwr.downloadHandler.data);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning("[Netowrk] error: " + e.Message);
                    }
                }
                return texture;
            }
            else
            {
                throw new Exception("[Netowrk] error: " + uwr.error + " " + uwr.uri);
            }
        }

        public static async Task<AudioClip> GetAudioClip(string url, CancellationTokenSource cancelationToken, System.Action<float> progress = null, bool caching = true, AudioType audioType = AudioType.OGGVORBIS)
        {
            string path = await url.GetPathOrUrl();
            bool isCached = path.Contains("file://");
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(path, audioType);
        
            UnityWebRequest uwr = await SendWebRequest(request, cancelationToken, isCached ? null : progress);
            if (uwr != null && !uwr.isHttpError && !uwr.isNetworkError)
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(uwr);
                audioClip.name = Path.GetFileName(uwr.url);
                if (caching && !isCached)
                {
                    try
                    {
                        ResourceCache.Caching(uwr.url, uwr.downloadHandler.data);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning("[Netowrk] error: " + e.Message);
                    }
                }
                return audioClip;
            }
            else
            {
                throw new Exception("[Netowrk] error: " + uwr.error + " " + uwr.uri);
            }
        }

        private delegate void AsyncOperation();

        public static async Task<string> GetVideoStream(string url, CancellationTokenSource cancelationToken, System.Action<float> progress = null, bool caching = true)
        {
            string path = await url.GetPathOrUrl();
            if (!path.Contains("file://"))
            {
                AsyncOperation cachingVideo = async delegate {
                    try
                    {
                        if (caching && ResourceCache.CheckFreeSpace(await GetSize(url)))
                        {   
                            ResourceCache.Caching(url, await GetData(url, cancelationToken, progress));
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning("[Netowrk] error: " + e.Message);
                    }
                };
                cachingVideo();
                return url;
            }
            else { return path; }
        }

        public static async Task<AssetBundle> GetAssetBundle(string url, CancellationTokenSource cancelationToken, System.Action<float> progress = null, bool caching = true)
        {
            UnityWebRequest request;
            CachedAssetBundle assetBundleVersion = await GetAssetBundleVersion(url);
            if (Caching.IsVersionCached(assetBundleVersion) || (caching && ResourceCache.CheckFreeSpace(await GetSize(url))))
            {
                request = UnityWebRequestAssetBundle.GetAssetBundle(url, assetBundleVersion, 0);
            }
            else 
            {
                request = UnityWebRequestAssetBundle.GetAssetBundle(url);
            }

            UnityWebRequest uwr = await SendWebRequest(request, cancelationToken, Caching.IsVersionCached(assetBundleVersion) ? null : progress);
            if (uwr != null && !uwr.isHttpError && !uwr.isNetworkError)
            {
                AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                if (caching) 
                {
                    // Deleting old versions from the cache
                    Caching.ClearOtherCachedVersions(assetBundle.name, assetBundleVersion.hash);
                }
                return assetBundle;
            }
            else
            {
                throw new Exception("[Netowrk] error: " + uwr.error + " " + uwr.uri);
            }
        }

        private static async Task<CachedAssetBundle> GetAssetBundleVersion(string url)
        {
            Hash128 hash = default;
            string localPath = new System.Uri(url).LocalPath;
            try
            {
                string manifest = await GetText(url + ".manifest");
                hash = manifest.GetHash128();
                return new CachedAssetBundle(localPath, hash);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Netowrk] error: " + e.Message);
                DirectoryInfo dir = new DirectoryInfo(url.ConvertToCachedPath());
                if (dir.Exists)
                {
                    System.DateTime lastWriteTime = default;
                    foreach (var item in dir.GetDirectories())
                    {
                        if (lastWriteTime < item.LastWriteTime)
                        {
                            if (hash.isValid && hash != default) 
                            { 
                                Directory.Delete(Path.Combine(dir.FullName, hash.ToString()), true);
                            }
                            lastWriteTime = item.LastWriteTime;
                            hash = Hash128.Parse(item.Name);
                        }
                        else { Directory.Delete(Path.Combine(dir.FullName, item.Name), true); }
                    }
                    return new CachedAssetBundle(localPath, hash);
                }
                else
                {
                    throw new Exception("[Netowrk] error: Nothing was found in the cache for " + url);
                }
            }
        }

        private static Hash128 GetHash128(this string str)
        {
            var hashRow = str.Split("\n".ToCharArray())[5];
            var hash = Hash128.Parse(hashRow.Split(':')[1].Trim());
            if (hash.isValid && hash != default) { return hash; }
            else { throw new Exception("[Netowrk] error: couldn't extract hash from manifest."); }
        }

        private static async Task<string> GetPathOrUrl(this string url)
        {
            string path = url.ConvertToCachedPath();
            if (File.Exists(path)) {
                if (Application.internetReachability != NetworkReachability.NotReachable)
                {
                    try
                    {
                        if (new FileInfo(path).Length != await GetSize(url)) { return url; }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("[Netowrk] error: " + e.Message);
                    }
                }
                return "file://" + path;
            }
            else return url;
        }

        public class Exception : System.Exception
        {
            public Exception(string message) : base(message)
            { }
        }
    }
}