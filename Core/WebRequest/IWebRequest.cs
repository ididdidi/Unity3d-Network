using System.Threading;
using System.Threading.Tasks;

namespace ru.ididdidi.Unity3D
{
    public interface IWebRequest
    {
        string url { get; set; }
        System.Action<float> ProgressHandler { get; }
        CancellationTokenSource CancelToken { get; }
        void Send();
        void Cancel();
    }
}