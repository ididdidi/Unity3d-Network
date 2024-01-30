using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ru.ididdidi.Unity3D
{
    /// <summary>
    /// Base wrapper class for UnityWebRequest.
    /// </summary>
    /// <typeparam name="T">Type of resource being processed</typeparam>
    public abstract partial class WebRequest<T> : IWebRequest
    {
        public string url { get; set; }
        public System.Action<T> Handler { get; private set; }
        public System.Action<float> ProgressHandler { get; private set; }
        public CancellationTokenSource CancelToken { get; }

        /// <summary>
        /// Base constructor.
        /// </summary>
        /// <param name="url">URL address</param>
        public WebRequest(string url)
        {
            this.url = url;
            this.CancelToken = new CancellationTokenSource();
        }

        /// <summary>
        /// Get the latest version.
        /// </summary>
        /// <returns>Version of the file <see cref="Hash128"/></returns>
        public async Task<Hash128> GetLatestVersion() => Hash128.Compute($"{url}{await UnityNetService.GetSize(url)}");

        /// <summary>
        /// Method for sending a web request.
        /// </summary>
        public abstract void Send();

        /// <summary>
        /// The method sets a delegate to display progress.
        /// </summary>
        /// <param name="progress">Delegate receiving <see cref="float"/></param>
        /// <returns>Instance of a specialized class</returns>
        public WebRequest<T> SetProgressHandler(System.Action<float> progress)
        {
            this.ProgressHandler = progress;
            return this;
        }

        /// <summary>
        /// The method adds a delegate to process the request.
        /// </summary>
        /// <param name="onResponse">Delegate to process the request</param>
        /// <returns>Instance of a specialized class</returns>
        public WebRequest<T> AddHandler(System.Action<T> onResponse)
        {
            this.Handler += onResponse;
            return this;
        }

        /// <summary>
        /// The method removes the delegate to process the request.
        /// </summary>
        /// <param name="onResponse">Delegate to process the request</param>
        /// <returns>Instance of a specialized class</returns>
        public WebRequest<T> RemoveHandler(System.Action<T> onResponse)
        {
            this.Handler -= onResponse;
            return this;
        }

        /// <summary>
        /// Cancel web request.
        /// </summary>
        public void Cancel() => CancelToken.Cancel();

        /// <summary>
        /// Exception wrapper.
        /// </summary>
        public class Exception : System.Exception
        {
            public Exception(string message) : base(message) { }
        }
    }
}