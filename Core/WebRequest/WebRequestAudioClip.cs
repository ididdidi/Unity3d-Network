using UnityEngine;
using UnityEngine.Networking;

namespace ru.ididdidi.Unity3D
{
    /// <summary>
    /// Specialized class for downloading <see cref="AudioClip"/>
    /// </summary>
    public class WebRequestAudioClip : WebRequest<AudioClip>
    {
        private AudioType audioType;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="url">URL address</param>
        /// <param name="audioType">Requested audio file type</param>
        public WebRequestAudioClip(string url, AudioType audioType = AudioType.OGGVORBIS) : base(url) { this.audioType = audioType; }

        /// <summary>
        /// Send a web request to the specified url.
        /// </summary>
        public override async void Send()
        {
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, audioType);

            AudioClip audioClip = await request.SendWebRequest(
                (response) => DownloadHandlerAudioClip.GetContent(request), CancelToken, ProgressHandler);

            audioClip.name = System.IO.Path.GetFileNameWithoutExtension(request.url);
            Handler?.Invoke(audioClip);
            request.Dispose();
        }
    }
}