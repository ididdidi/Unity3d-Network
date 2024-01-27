using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    public static class UnityNetService
    {
        public delegate T Handler<T>(UnityWebRequest request);
        public static async Task<T> SendWebRequest<T>(this UnityWebRequest request, Handler<T> handler, CancellationTokenSource cancelToken = null, System.Action<float> progress = null)
        {
            while (!Caching.ready) { await Task.Yield(); }

#pragma warning disable CS4014
            request.SendWebRequest();
#pragma warning restore CS4014

            while (!request.isDone)
            {
                if (cancelToken != null && cancelToken.IsCancellationRequested)
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
                return handler.Invoke(request);
            }
            else
            {
                throw new Exception($"Request - {request.error} {request.url}");
            }
        }

        public static async Task<long> GetSize(string url)
        {
            return await UnityWebRequest.Head(url).SendWebRequest((response) =>
            {
                string contentLength = response.GetResponseHeader("Content-Length");
                if (long.TryParse(contentLength, out long returnValue))
                {
                    return returnValue;
                }
                else
                {
                    throw new Exception(string.Format("GetSize - {0} {1}", response?.error, url));
                }
            });
        }

        public static async Task<byte[]> GetData(string url, CancellationTokenSource cancelToken = null, System.Action<float> progress = null)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            return await request.SendWebRequest((response) => response.downloadHandler.data, cancelToken, progress);
        }

        public class Exception : System.Exception
        {
            public Exception(string message) : base(message) { }
        }
    }
}