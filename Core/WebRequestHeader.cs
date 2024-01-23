using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    public class WebRequestHeader : WebRequest<Dictionary<string, string>>
    {
        public WebRequestHeader(string url) : base(url) { }

        public override async Task Send()
        {
            UnityWebRequest request = await Send(UnityWebRequest.Head(url));
            Response?.Invoke(request?.GetResponseHeaders());
        }
    }
}