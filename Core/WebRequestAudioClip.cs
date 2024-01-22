using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    public class WebRequestAudioClip : WebRequest<AudioClip>
    {
        protected AudioType audioType = AudioType.OGGVORBIS;
        public WebRequestAudioClip(string url) : base(url) { }

        public WebRequestAudioClip SetAudioType(AudioType audioType)
        {
            this.audioType = audioType;
            return this;
        }

        public override async Task Send()
        {
            UnityWebRequest request = await Send(UnityWebRequestMultimedia.GetAudioClip(url, audioType), progress);
            if(request != null)
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(request);
                audioClip.name = System.IO.Path.GetFileNameWithoutExtension(request.url);
                response(audioClip);
            }
        }
    }
}