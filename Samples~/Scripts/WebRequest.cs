using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    public abstract class WebRequest<T> : IRequest
    {
        protected string url;
        protected System.Action<T> response;
        protected System.Action<float> progress;
        protected CancellationTokenSource cancellationToken;

        public WebRequest(string url)
        {
            this.url = url;
            this.cancellationToken = new CancellationTokenSource();
        }

        public abstract Task Send();

        public WebRequest<T> SetProgress(System.Action<float> progress)
        {
            this.progress = progress;
            return this;
        }

        public WebRequest<T> AddResponse(System.Action<T> response)
        {
            this.response += response;
            return this;
        }

        public WebRequest<T> RemoveResponse(System.Action<T> response)
        {
            this.response -= response;
            return this;
        }

        public async Task<long> GetSize()
        {
            UnityWebRequest request = await Send(UnityWebRequest.Head(url));
            var contentLength = request.GetResponseHeader("Content-Length");
            if (long.TryParse(contentLength, out long returnValue))
            {
                return returnValue;
            }
            else
            {
                throw new Exception(string.Format("Netowrk.GetSize - {0} {1}", request.error, url));
            }
        }

        protected async Task<UnityWebRequest> Send(UnityWebRequest request,  System.Action<float> progress = null)
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