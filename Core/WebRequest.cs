using System.Threading;
using UnityEngine;

namespace ru.ididdidi.Unity3D
{
    public abstract partial class WebRequest<T> : IWebRequest
    {
        protected Hash128 hash = default;

        protected System.Action<T> handler;
        private System.Action<float> progress;

        public string url { get; set; }
        public System.Action<T> Handler { get => handler; }
        public System.Action<float> Progress { get => progress; }
        public CancellationTokenSource CancelToken { get; }

        public WebRequest(string url)
        {
            this.url = url;
            this.CancelToken = new CancellationTokenSource();
        }

        public abstract void Send();

        public WebRequest<T> SetProgress(System.Action<float> progress)
        {
            this.progress = progress;
            return this;
        }

        public WebRequest<T> AddResponseHandler(System.Action<T> onResponse)
        {
            this.handler += onResponse;
            return this;
        }

        public WebRequest<T> RemoveResponseHandler(System.Action<T> onResponse)
        {
            this.handler -= onResponse;
            return this;
        }

        public void Cancel() => CancelToken.Cancel();

        public class Exception : System.Exception
        {
            public Exception(string message) : base(message) { }
        }
    }
}