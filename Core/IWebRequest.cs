using System.Threading;

namespace ru.ididdidi.Unity3D
{
    public interface IWebRequest
    {
        string url { get; set; }
        System.Action<float> Progress { get; }
        CancellationTokenSource CancelToken { get; }
        void Send();
        void Cancel();
    }
}