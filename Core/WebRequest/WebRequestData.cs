using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    public class WebRequestData : WebRequest<byte[]>
    {
        public WebRequestData(string url) : base(url) { }

        public override async void Send()
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            Handler?.Invoke(await request.SendWebRequest((response) => response.downloadHandler.data, CancelToken, ProgressHandler));
            request.Dispose();
        }
    }
}