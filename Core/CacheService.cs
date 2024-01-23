using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class CacheService
{
    private const float MIB = 1048576f;
    private static string cachingDirectory = "cache";

    public static string CachingDirectory { get => cachingDirectory; }

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

    public static string ConvertToCachedPath(this string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            string localPath = new System.Uri(url).LocalPath;
            return Path.Combine(Application.persistentDataPath, $"{cachingDirectory}{localPath}").Replace("\\", "/");
        }
        else
        {
            throw new Exception($"ConvertToCachedPath - Url address was entered incorrectly {url}");
        }
    }

    public static bool IsCached(string path, long size)
    {
        if (File.Exists(path))
        {
            if (new FileInfo(path).Length != size) { return false; }
            return true;
        }
        return false;
    }

    public static string Caching(string url, byte[] data)
    {
        if (CheckFreeSpace(data.Length))
        {
            string path = url.ConvertToCachedPath();

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
