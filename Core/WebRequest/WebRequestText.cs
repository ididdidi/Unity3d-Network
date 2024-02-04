using UnityEngine.Networking;

namespace UnityNetwork
{
    /// <summary>
    /// Specialized class for downloading <see cref="string"/>
    /// </summary>
    public class WebRequestText : WebRequest<string>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="url">URL address</param>
        public WebRequestText (string url) : base(url) { }

        /// <summary>
        /// Send a web request to the specified url.
        /// </summary>
        public override async void Send()
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            Handler?.Invoke(await request.SendWebRequest((response) => response.downloadHandler.text));
            request.Dispose();
        }
    }
}