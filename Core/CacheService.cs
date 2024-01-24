using System.IO;
using UnityEngine;

public static class CacheService
{
    private const float MIB = 1048576f;
    private static string cachingDirectory = "cache";

    public static string CachingDirectory { get => cachingDirectory; }
    public static bool Caching { get; set; }

    static CacheService() => ConfiguringCaching(cachingDirectory);

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

    public static string ConvertToCachedPath(this string url, Hash128 version)
    {
        if (!string.IsNullOrEmpty(url))
        {
            string[] path = {
                Application.persistentDataPath,
                cachingDirectory,
                Hash128.Compute(url).ToString(),
                version.ToString(),
                Path.GetFileName(url).Replace("\\", "/")
            };
            return Path.Combine(path);
        }
        else
        {
            throw new Exception($"ConvertToCachedPath - Url address was entered incorrectly {url}");
        }
    }

    public static Hash128 GetCachedVersion(string url)
    {
        Hash128 version = default;
        DirectoryInfo dir = new DirectoryInfo(url.ConvertToCachedPath(version));
        if (dir.Exists)
        {
            System.DateTime lastWriteTime = default;
            var dirs = dir.GetDirectories();
            for (int i = 0; i < dirs.Length; i++)
            {
                if (lastWriteTime < dirs[i].LastWriteTime)
                {
                    if (version.isValid && version != default)
                    {
                        Directory.Delete(Path.Combine(dir.FullName, version.ToString()), true);
                    }
                    lastWriteTime = dirs[i].LastWriteTime;
                    version = Hash128.Parse(dirs[i].Name);
                }
                else { Directory.Delete(Path.Combine(dir.FullName, dirs[i].Name), true); }
            }
        }
        return version;
    }

    public static bool IsCached(string url, Hash128 version) 
        => new FileInfo(url.ConvertToCachedPath(version)).Exists;


    public static string SeveToCache(string url, Hash128 version, byte[] data)
    {
        if (CheckFreeSpace(data.Length))
        {
            string path = url.ConvertToCachedPath(version);

            DirectoryInfo dirInfo = new DirectoryInfo(Application.persistentDataPath);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
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
