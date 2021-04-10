﻿using System.IO;
using UnityEngine;

namespace ru.mofrison.Network
{
    public static class ResourceCache
    {
        private const float MIB = 1048576f;
        private static string cachingDirectory = "data";

        public static void ConfiguringCaching(string directoryName)
        {
            cachingDirectory = directoryName;
            var path = Path.Combine(Application.persistentDataPath, cachingDirectory);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            UnityEngine.Caching.currentCacheForWriting = UnityEngine.Caching.AddCache(path);
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

        public static void Caching(string url, byte[] data)
        {
            if (url.Contains("file://")) return;

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
            }
            else { throw new Exception("[Caching] error: Not available space to download " + data.Length / MIB + "Mb"); }
        }

        public static string ConvertToCachedPath(this string url)
        {
            try
            {
                if (!string.IsNullOrEmpty(url))
                {
                    var path = Path.Combine(Application.persistentDataPath, cachingDirectory + new System.Uri(url).LocalPath);
                    return path.Replace("\\", "/");
                }
                else
                {
                    throw new Exception("[Caching] error: Url address was entered incorrectly " + url); ;
                }
            }
            catch (System.UriFormatException e)
            {
                throw new Exception("[Caching] error: " + url + " " + e.Message);
            }
        }

        public class Exception : System.Exception
        {
            public Exception(string message) : base(message)
            { }
        }
    }
}