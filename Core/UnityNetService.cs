using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    /// <summary>
    /// Class for asynchronously sending web requests.
    /// </summary>
    public static class UnityNetService
    {
        /// <summary>
        /// Web request handler that extracts data of a specific type from the response and returns it.
        /// </summary>
        /// <typeparam name="T">Type of data extracted from response</typeparam>
        /// <param name="request"><see cref="UnityWebRequest"/></param>
        /// <returns>Data received as a result of a response to a web request</returns>
        public delegate T Handler<T>(UnityWebRequest request);

        /// <summary>
        /// Method for sending an asynchronous web request
        /// </summary>
        /// <typeparam name="T">Type of data extracted from response</typeparam>
        /// <param name="request"><see cref="UnityWebRequest"/></param>
        /// <param name="handler">Web request handler that extracts data of a specific type from the response and returns it</param>
        /// <param name="cancelToken">Token for canceling a web request</param>
        /// <param name="progress">Delegate to visualize progress</param>
        /// <returns>Data received as a result of a response to a web request</returns>
        public static async Task<T> SendWebRequest<T>(this UnityWebRequest request, Handler<T> handler, CancellationTokenSource cancelToken = null, System.Action<float> progress = null)
        {
            while (!Caching.ready) { await Task.Yield(); }

#pragma warning disable CS4014
            request.SendWebRequest();
#pragma warning restore CS4014

            while (!request.isDone)
            {
                if (Application.isPlaying && cancelToken != null && cancelToken.IsCancellationRequested)
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

            if (Application.isPlaying && !request.isHttpError && !request.isNetworkError)
            {
                progress?.Invoke(1f);
                return handler.Invoke(request);
            }
            else
            {
                throw new Exception($"Request - {request.error} {request.url}");
            }
        }

        /// <summary>
        /// Method to get file size using web request
        /// </summary>
        /// <param name="url">File URL</param>
        /// <returns>Size in bytes</returns>
        public static async Task<long> GetSize(string url)
        {
            if (url.Contains("file://"))
            {
                return new System.IO.FileInfo(new System.Uri(url).LocalPath).Length;
            }

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

        /// <summary>
        /// Method for downloading a file via a web request
        /// </summary>
        /// <param name="url">File URL</param>
        /// <param name="cancelToken">Token for canceling a web request</param>
        /// <param name="progress"></param>
        /// <returns>Delegate to visualize progress</returns>
        public static async Task<byte[]> GetData(string url, CancellationTokenSource cancelToken = null, System.Action<float> progress = null)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            return await request.SendWebRequest((response) => response.downloadHandler.data, cancelToken, progress);
        }

        /// <summary>
        /// Exception wrapper.
        /// </summary>
        public class Exception : System.Exception
        {
            public Exception(string message) : base(message) { }
        }
    }
}