using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    public class WebRequestData : WebRequest<byte[]>
    {
        public WebRequestData(string url) : base(url) { }

        public override async Task Send()
        {
            UnityWebRequest request = await Send(UnityWebRequest.Get(url), progress);
            Response?.Invoke(request?.downloadHandler.data);
        }
    }
}