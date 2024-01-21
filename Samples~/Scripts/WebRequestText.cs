using System.Threading.Tasks;
using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    public class WebRequestText : WebRequest<string>
    {
        public WebRequestText (string url) : base(url) { }

        public override async Task Send()
        {
            UnityWebRequest request = await Send(UnityWebRequest.Get(url));
            response?.Invoke(request?.downloadHandler.text);
        }
    }
}