using System.IO;
using UnityEngine;

namespace ru.ididdidi.Unity3D
{
    public static class UnityCacheService
    {
        private const float MIB = 1048576f;
        private static string cachingDirectory = "cache";

        public static bool Caching { get; set; }

        static UnityCacheService() => ConfiguringCaching(cachingDirectory);

        private static void ConfiguringCaching(string directoryName)
        {
            cachingDirectory = directoryName;
            var path = Path.Combine(Application.persistentDataPath, cachingDirectory);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            UnityEngine.Caching.currentCacheForWriting = UnityEngine.Caching.AddCache(path);
        }

        public static string GetCachedDirectory(this string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                string[] path = {
                Application.persistentDataPath,
                cachingDirectory,
                Hash128.Compute(url).ToString()
            };
                return Path.Combine(path).Replace("\\", "/");
            }
            else
            {
                throw new Exception($"ConvertToCachedPath - Url address was entered incorrectly {url}");
            }
        }

        public static string GetCachedPath(this string url, Hash128 version)
        {
            return Path.Combine(url.GetCachedDirectory(), version.ToString(), Path.GetFileName(url)).Replace("\\", "/");
        }

        public static Hash128 GetCachedVersion(string url)
        {
            Hash128 version = default;
            DirectoryInfo dir = new DirectoryInfo(url.GetCachedDirectory());
            if (dir.Exists)
            {
                System.DateTime lastWriteTime = default;
                var dirs = dir.GetDirectories();
                for (int i = 0; i < dirs.Length; i++)
                {
                    if (lastWriteTime < dirs[i].LastWriteTime)
                    {
                        lastWriteTime = dirs[i].LastWriteTime;
                        version = Hash128.Parse(dirs[i].Name);
                    }
                }
            }
            return version;
        }

        public static bool IsCached(string url, Hash128 version)
            => new FileInfo(url.GetCachedPath(version)).Exists;

        public static void GetFromCache(this IWebRequest request, Hash128 version)
        {
            request.url = request.url.GetCachedPath(version);
            request.Send();
        }

        public static string SeveToCache(string url, Hash128 version, byte[] data, bool clearOldVersions = true)
        {
            if (CheckFreeSpace(data.Length))
            {

                DirectoryInfo dirInfo = new DirectoryInfo(url.GetCachedDirectory());
                if (clearOldVersions && dirInfo.Exists) { dirInfo.Delete(true); }
                dirInfo.Create();

                string path = url.GetCachedPath(version);
                dirInfo.CreateSubdirectory(Directory.GetParent(path).FullName);
                File.WriteAllBytes(path, data);
                return path;
            }
            else { throw new Exception(string.Format("Caching - Not available space to download {0}Mb", data.Length / MIB)); }
        }

        public static bool CheckFreeSpace(float sizeInBytes)
        {
#if UNITY_EDITOR_WIN
            var logicalDrive = Path.GetPathRoot(Application.persistentDataPath);
            var availableSpace = SimpleDiskUtils.DiskUtils.CheckAvailableSpace(logicalDrive);
#elif UNITY_EDITOR_OSX
        var availableSpace = SimpleDiskUtils.DiskUtils.CheckAvailableSpace();
#elif UNITY_IOS
        var availableSpace = SimpleDiskUtils.DiskUtils.CheckAvailableSpace();
#elif UNITY_ANDROID
        var availableSpace = SimpleDiskUtils.DiskUtils.CheckAvailableSpace(true);
#endif
            return availableSpace > sizeInBytes / MIB;
        }

        public class Exception : System.Exception
        {
            public Exception(string message) : base(message)
            { }
        }
    }
}
