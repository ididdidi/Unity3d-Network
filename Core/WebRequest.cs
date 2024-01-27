using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ru.ididdidi.Unity3D
{
    public abstract partial class WebRequest<T> : IWebRequest
    {
        public string url { get; set; }
        public System.Action<T> Handler { get; private set; }
        public System.Action<float> ProgressHandler { get; private set; }
        public CancellationTokenSource CancelToken { get; }

        public WebRequest(string url)
        {
            this.url = url;
            this.CancelToken = new CancellationTokenSource();
        }

        public async Task<Hash128> GetLatestVersion() => Hash128.Compute($"{url}{await UnityNetService.GetSize(url)}");

        public abstract void Send();

        public WebRequest<T> SetProgressHandler(System.Action<float> progress)
        {
            this.ProgressHandler = progress;
            return this;
        }

        public WebRequest<T> AddHandler(System.Action<T> onResponse)
        {
            this.Handler += onResponse;
            return this;
        }

        public WebRequest<T> RemoveHandler(System.Action<T> onResponse)
        {
            this.Handler -= onResponse;
            return this;
        }

        public void Cancel() => CancelToken.Cancel();

        public class Exception : System.Exception
        {
            public Exception(string message) : base(message) { }
        }
    }
}