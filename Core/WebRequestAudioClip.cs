using UnityEngine;
using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    public class WebRequestAudioClip : WebRequest<AudioClip>
    {
        AudioType audioType;

        public WebRequestAudioClip(string url, AudioType audioType = AudioType.OGGVORBIS) : base(url) { this.audioType = audioType; }

        public override async void Send()
        {
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, audioType);

            AudioClip audioClip = await request.SendWebRequest(
                (response) => DownloadHandlerAudioClip.GetContent(request), CancelToken, Progress);

            audioClip.name = System.IO.Path.GetFileNameWithoutExtension(request.url);
            handler?.Invoke(audioClip);
            request.Dispose();
        }
    }
}