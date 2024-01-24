using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    public abstract class WebRequest<T> : IRequest
    {
        protected Hash128 hash = default;
        protected System.Action<T> onResponse;
        protected System.Action<float> progress;
        protected CancellationTokenSource cancellationToken;

        public string url { get; }
        public System.Action<T> OnResponse { get => onResponse; }

        public WebRequest(string url)
        {
            this.url = url;
            this.cancellationToken = new CancellationTokenSource();
        }

        protected virtual async Task<Hash128> GetLatestVersion() => Hash128.Compute($"{url}{await GetSize()}");

        public async Task<Hash128> GetVersion()
        {
            if(hash.Equals(default))
            {
                try
                {
                    hash = await GetLatestVersion();
                }
                catch
                {
                    hash = CacheService.GetCachedVersion(url);
                }
            }
            return hash;
        }

        protected abstract Task<UnityWebRequest> GetWebResponse();

        protected abstract Task<UnityWebRequest> GetCacheResponse();

        public async Task Send()
        {
            UnityWebRequest response = await IsCached() ? await GetCacheResponse() : await GetWebResponse();
            if (response != null) { HandleResponse(response); }
        }

        public WebRequest<T> SetProgress(System.Action<float> progress)
        {
            this.progress = progress;
            return this;
        }

        public WebRequest<T> AddResponseHandler(System.Action<T> onResponse)
        {
            this.onResponse += onResponse;
            return this;
        }

        public WebRequest<T> RemoveResponseHandler(System.Action<T> onResponse)
        {
            this.onResponse -= onResponse;
            return this;
        }

        public async Task<long> GetSize()
        {
            UnityWebRequest request = await GetResponse(UnityWebRequest.Head(url));
            string contentLength = request.GetResponseHeader("Content-Length");
            if (long.TryParse(contentLength, out long returnValue))
            {
                return returnValue;
            }
            else
            {
                throw new Exception(string.Format("GetSize - {0} {1}", request?.error, url));
            }
        }

        public virtual async Task<bool> IsCached() => CacheService.IsCached(url, await GetVersion());

        protected abstract void HandleResponse(UnityWebRequest response);

        protected async Task<UnityWebRequest> GetResponse(UnityWebRequest request,  System.Action<float> progress = null)
        {
            while (!Caching.ready)
            {
                if (cancellationToken != null && cancellationToken.IsCancellationRequested)
                {
                    Debug.LogWarning($"Request {request.url} has been canceled.");
                    return null;
                }
                await Task.Yield();
            }

#pragma warning disable CS4014
            request.SendWebRequest();
#pragma warning restore CS4014

            while (!request.isDone)
            {
                if (cancellationToken != null && cancellationToken.IsCancellationRequested)
                {
                    request.Abort();
                    var url = request.url;
                    request.Dispose();
                    throw new Exception($"Request {url} - cancel download: {0}");
                }
                else
                {
                    progress?.Invoke(request.downloadProgress);
                    await Task.Yield();
                }
            }

            if (!request.isHttpError && !request.isNetworkError)
            {
                progress?.Invoke(1f);
                return request;
            }
            else
            {
                throw new Exception($"Request - {request.error} {request.url}");
            }
        }

        public void Cancel() => cancellationToken.Cancel();

        public class Exception : System.Exception
        {
            public Exception(string message) : base(message) { }
        }
    }
}