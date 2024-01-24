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

        protected override async Task<UnityWebRequest> GetWebResponse()
        {
            UnityWebRequest responce = await GetResponse(UnityWebRequestMultimedia.GetAudioClip(url, audioType), progress);

            if (CacheService.Caching)
            {
                CacheService.SeveToCache(url, await GetVersion(), responce.downloadHandler.data) ;
            }

            return responce;
        }

        protected override async Task<UnityWebRequest> GetCacheResponse()
        {
            string path = url.ConvertToCachedPath(await GetVersion());
            return await GetResponse(UnityWebRequestMultimedia.GetAudioClip(path, audioType));
        }

        protected override void HandleResponse(UnityWebRequest response)
        {
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(response);
            audioClip.name = System.IO.Path.GetFileNameWithoutExtension(response.url);
            onResponse?.Invoke(audioClip);
        }
    }
}