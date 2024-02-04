using System.Threading;

namespace UnityNetwork
{
    /// <summary>
    /// Interface for interacting with a web request.
    /// </summary>
    public interface IWebRequest
    {
        /// <summary>
        /// URL address.
        /// </summary>
        string url { get; set; }
        /// <summary>
        /// Token to cancel request.
        /// </summary>
        CancellationTokenSource CancelToken { get; }
        /// <summary>
        /// Send a web request to the specified url.
        /// </summary>
        void Send();
        /// <summary>
        /// Cancel web request.
        /// </summary>
        void Cancel();
    }
}