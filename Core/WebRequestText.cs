using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    public class WebRequestText : WebRequest<string>
    {
        public WebRequestText (string url) : base(url) { }

        public override async void Send()
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            handler?.Invoke(await request.SendWebRequest((response) => response.downloadHandler.text));
            request.Dispose();
        }
    }
}