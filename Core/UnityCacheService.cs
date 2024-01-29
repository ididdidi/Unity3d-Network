using System.IO;
using UnityEngine;

namespace ru.ididdidi.Unity3D
{
    public static class UnityCacheService
    {
        private const float MIB = 1048576f;
        private static string cachingDirectory = "cache";

        /// <summary>
        /// Flag for caching resources downloaded via a web request.
        /// </summary>
        public static bool Caching { get; set; }

        /// <summary>
        /// Static constructor that runs the first time the class is accessed.
        /// </summary>
        static UnityCacheService() => ConfiguringCaching(cachingDirectory);

        /// <summary>
        /// Cache configuration method.
        /// </summary>
        /// <param name="directoryName">Caching directory name</param>
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

        /// <summary>
        /// Returns the full path to the cache directory.
        /// </summary>
        /// <param name="url">File URL</param>
        /// <returns>Full path to the cache directory</returns>
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

        /// <summary>
        /// Method for get a full path to the cache file.
        /// </summary>
        /// <param name="url">File URL</param>
        /// <param name="version">Cached version of the file</param>
        /// <returns>Full path to the cache file</returns>
        public static string GetCachedPath(this string url, Hash128 version)
        {
            return Path.Combine(url.GetCachedDirectory(), version.ToString(), Path.GetFileName(url)).Replace("\\", "/");
        }

        /// <summary>
        /// Method for get a cached version of the file.
        /// </summary>
        /// <param name="url">File URL</param>
        /// <returns>Cached version of the file</returns>
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

        /// <summary>
        /// The method that checks whether a file of a given version is in the cache.
        /// </summary>
        /// <param name="url">File URL</param>
        /// <param name="version">Cached version of the file</param>
        /// <returns>The version file is in the cache</returns>
        public static bool IsCached(string url, Hash128 version) => new FileInfo(url.GetCachedPath(version)).Exists;

        /// <summary>
        /// A method for storing data in a device's long-term memory.
        /// </summary>
        /// <param name="url">File URL</param>
        /// <param name="version">Cached version of the file</param>
        /// <param name="data">Data as <see cref="byte[]"/></param>
        /// <param name="clearOldVersions">Clear previous versions flag</param>
        /// <returns>File data as <see cref="byte[]"/></returns>
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

        /// <summary>
        /// Method to check free space on device.
        /// </summary>
        /// <param name="sizeInBytes">Required space in bytes</param>
        /// <returns>Is there enough memory for this length on the device</returns>
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

        /// <summary>
        /// Exception wrapper.
        /// </summary>
        public class Exception : System.Exception
        {
            public Exception(string message) : base(message)
            { }
        }
    }
}
