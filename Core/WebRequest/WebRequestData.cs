using UnityEngine.Networking;

namespace UnityNetwork
{
    /// <summary>
    /// Specialized class for downloading data as <see cref="byte[]"/>
    /// </summary>
    public class WebRequestData : WebRequest<byte[]>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="url">URL address</param>
        public WebRequestData(string url) : base(url) { }

        /// <summary>
        /// Send a web request to the specified url.
        /// </summary>
        public override async void Send()
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            Handler?.Invoke(await request.SendWebRequest((response) => response.downloadHandler.data, CancelToken, ProgressHandler));
            request.Dispose();
        }
    }
}