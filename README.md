# Unity3d-Netowrk
Package for downloading resources from the Internet into Unity.

## Install
You can add this repository to your Unity project using the [Package Manager](https://learn.unity.com/tutorial/the-package-manager).
To do this, click `Window` in the top panel, select `Package Manager` in the drop-down list, in the window that opens in the upper left corner, click __`+`__ and select `Add package from git-URL...`.
In the field that appears, paste:

	https://github.com/ididdidi/Unity3d-Network.git

Press enter end wait for the package to download.
  
## Network.cs
I created a static class `Network.cs` in which I defined the main methods for loading various types of resources.

Method name                                     | Implemented function
------------------------------------------------|-------------------------
[SendWebRequest](#sendwebrequest)               | It is used for interaction with the network: sending requests and receiving data.
[GetSize](#getsize)                             | It is used to find out the size of a file in external storage.
[GetText](#gettext)                             | It is intended for downloading files in text format.
[GetData](#getdata)                             | Allows you to get data from the network in the form of a `byte ' masiiv.
[GetTexture](#gettexture)                       | Implements downloading images with the ability to save them to the cache.
[GetAudioClip](#getaudioclip)                   | Provides downloading of an audio recording with the ability to save it to the cache.
[GetVideoStream](#getvideostream)               | Provides the address of the video file to play as a stream.
[GetAssetBundle](#getassetbundle)               | Loads pre-prepared **AssetBundle** files.
[GetAssetBundleVersion](#getassetbundleversion) | Determines the current version **AssetBundle**. 
[GetHash128](#gethash128)                       | It is used to extract the hash from the manifest
[GetCachedPath](#getcachedpath)                 | Used to get the path to the encrypted file, if any.

Methods for downloading cached resources (other than GetAssetBundle) use methods provided by the **ResourceCache** static class.
**ResourceCache** helps you store data in your device's memory and interact with it. It is discussed in detail in the [Unity3d: Saving data on the device](https://ididdidi.ru/cases/unity3d-caching-resources).

### SendWebRequest
The main one for the `Network` class is the private asynchronous method `WebRequest`, which is used to load data from the network. As arguments, it accepts a pre-prepared `Unity Web Request`, `CancellationTokenSource`, `Action<float>`.
```csharp
private static async Task<UnityWebRequest> WebRequest(UnityWebRequest request, CancellationTokenSource cancelationToken = null, System.Action<float> progress = null)
{
    while (!Caching.ready)
    {
        if (cancelationToken != null && cancelationToken.IsCancellationRequested)
        {
            return null;
        }
        await Task.Yield();
    }

#pragma warning disable CS4014
    request.SendWebRequest();
#pragma warning restore CS4014

    while (!request.isDone)
    {
        if (cancelationToken != null && cancelationToken.IsCancellationRequested)
        {
            request.Abort();
            request.Dispose();
            return null;
        }
        else
        {
            progress?.Invoke(request.downloadProgress);
            await Task.Yield();
        }
    }

    progress?.Invoke(1f);
    return request;
}
```
As you can see, the method body starts with a loop waiting for the cache to be ready. Next, the request itself is sent: `request.Send WebRequest()`. After that, a loop is executed waiting for the execution of this very request. During this loop, the progress of this operation is passed to the calling method via `Action<float>`. And at the end of the method, the request is returned to the calling method.

In order for the process of waiting for the cache and executing the request to occur asynchronously, a call `await Task.Yield()` was added to the loops, the `async` modifier was added to the method name, and the returned `UnityWebRequest` was wrapped in `Task<>`.  About `await Task.Yield()` I learned from the article [Looking into Unity's async/await](https://gametorrahod.com/unity-and-async-await/). Previously, I used to call `await new WaitForEndOfFrame()` from the **AsyncAwaitUtil** plugin instead, but decided to abandon it in order to reduce external dependencies.

In addition, in both loops, the `cancellationtoken.IsCancellationRequested` flag is checked, through which the method can be informed about the need to abort the execution of operations and return `null`.

### GetSize
This method is used to find out the size of a file in external storage. Accepts a string with the file url as the only argument.
```csharp
public static async Task<long> GetSize(string url)
{
    UnityWebRequest request = await SendWebRequest(UnityWebRequest.Head(url));
    var contentLength = request.GetResponseHeader("Content-Length");
    if (long.TryParse(contentLength, out long returnValue))
    {
        return returnValue;
    }
    else
    {
        throw new Exception("[Netowrk] error: " + request.error + " " + url);
    }
}
```
The method asynchronously sends a 'Unity Web Request' request to get the header and waits for it to be executed. Once the request is executed, from it using the `GetResponseHeader("Content-Length")` method text is extracted, which is converted to chilo by the `long.TryParse` method. The number is returned to the calling method as `Task<long>`.

### GetText
This method allows you to download files in text format. Accepts a string with the file url as the only argument.
```csharp
public static async Task<string> GetText(string url)
{
    var uwr = await SendWebRequest(UnityWebRequest.Get(url));
    if (uwr != null && !uwr.isHttpError && !uwr.isNetworkError)
    {
        return uwr.downloadHandler.text;
    }
    else
    {
        throw new Exception("[Netowrk] error: " + uwr.error + " " + uwr.url);
    }
}
```
This method allows you to download files in text format. Accepts a string with the file url as the only argument.When this method is called, the standard `UnityWebRequest` request is sent asynchronously and is expected to be executed. After the request is executed, the text is extracted from it and returned as a string.

### GetData
The `getData` method allows you to get data from the network in the form of a byte array. As input, it accepts the file url as a string, a token to interrupt the download, and an `Action` to display the download progress.
```csharp
public static async Task<byte[]> GetData(string url, CancellationTokenSource cancelationToken, System.Action<float> progress = null)
{
    UnityWebRequest uwr = await SendWebRequest(UnityWebRequest.Get(url), cancelationToken, progress);
    if (uwr != null && !uwr.isHttpError && !uwr.isNetworkError)
    {
        return uwr.downloadHandler.data;
    }
    else
    {
        throw new Exception("[Netowrk] error: " + uwr.error + " " + uwr.uri);
    }
}
```
The method sends a request via the `Send WebRequest` method described earlier. The method is also asynchronous, and the output is a `Task` with a `byte` array.

### GetTexture
The method for getting an image in the form of `Texture 2D`, takes as arguments a string with a url, `CancelationToken`, `Action` to display progress, and a flag indicating whether to cache the uploaded image or not.
```csharp
public static async Task<Texture2D> GetTexture(string url, CancellationTokenSource cancelationToken, System.Action<float> progress = null, bool caching = true)
{
    string path = await url.GetCachedPath();
    bool isCached = path.Contains("file://");
    UnityWebRequest request = UnityWebRequestTexture.GetTexture(path);

    UnityWebRequest uwr = await SendWebRequest(request, cancelationToken, isCached? null : progress);
    if (uwr != null && !uwr.isHttpError && !uwr.isNetworkError)
    {
        Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
        texture.name = Path.GetFileName(uwr.url);
        if (caching && !isCached) 
        {
            try
            {
                ResourceCache.Caching(uwr.url, uwr.downloadHandler.data);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Netowrk] error: " + e.Message);
            }
        }
        return texture;
    }
    else
    {
        throw new Exception("[Netowrk] error: " + uwr.error + " " + uwr.uri);
    }
}
```
In this method, a specialized request is used to download the image `UnityWebRequestTexture.GetTexture(url)`. In addition, before sending the request, the presence of this file in the device memory is checked. To do this, use the extension method `GetCachedPath(this string path)`. If a file with the same name and the same size is located on a similar local path, the url is substituted for the path to this file. Метод возвращает объект типа `Texture2D`, обёрнутый в `Task`.The method returns an object of type `Texture2D` wrapped in a `Task`.

### GetAudioClip
The method for storing an audio file in the form of `AudioClip`, takes as arguments a string with a url, `CancelationToken`, `Action` to display progress, a flag indicating whether to cache the downloaded audio file or not, and `AudioType`, which determines the format of the audio recording. Preferred format: **OGG**
```csharp
public static async Task<AudioClip> GetAudioClip(string url, CancellationTokenSource cancelationToken, System.Action<float> progress = null, bool caching = true, AudioType audioType = AudioType.OGGVORBIS)
{
    string path = await url.GetCachedPath();
    bool isCached = path.Contains("file://");
    UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(path, audioType);
        
    UnityWebRequest uwr = await SendWebRequest(request, cancelationToken, isCached ? null : progress);
    if (uwr != null && !uwr.isHttpError && !uwr.isNetworkError)
    {
        AudioClip audioClip = DownloadHandlerAudioClip.GetContent(uwr);
        audioClip.name = Path.GetFileName(uwr.url);
        if (caching && !isCached)
        {
            try
            {
                ResourceCache.Caching(uwr.url, uwr.downloadHandler.data);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Netowrk] error: " + e.Message);
            }
        }
        return audioClip;
    }
    else
    {
        throw new Exception("[Netowrk] error: " + uwr.error + " " + uwr.uri);
    }
}
```
This method uses a specialized request to download an audio file: `UnityWebRequestMultimedia.GetAudioClip(url, audioType)`. In addition, before sending the request, the presence of this file in the device memory is checked. To do this, use the extension method: `GetCachedPath(this string paht)`. If a file with the same name and the same size is located on a similar local path, the url is substituted for the path to this file. The method returns an object of the `AudioClip` type wrapped in a `Task`.

### GetVideoStream
The method provides the path to the video file on the device if it was previously cached. Takes as arguments a string with a url, `cancelationToken`, `Action` to display the progress, and a flag indicating whether to cache the downloaded audio file or not.
```csharp
private delegate void AsyncOperation();

public static async Task<string> GetVideoStream(string url, CancellationTokenSource cancelationToken, System.Action<float> progress = null, bool caching = true)
{
    string path = await url.GetCachedPath();
    if (!path.Contains("file://"))
    {
        AsyncOperation cachingVideo = async delegate {
            try
            {
                if (caching && ResourceCache.CheckFreeSpace(await GetSize(url)))
                {
                    ResourceCache.Caching(url, await GetData(url, cancelationToken, progress));
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Netowrk] error: " + e.Message);
            }
        };
        cachingVideo();
        return url;
    }
    else { return path; }
}
```
The method first checks whether this file is in the device memory. If a file with the same name and size is located in a similar local path, the path to the file on the device is returned. If there is no such file, the file url is returned and the file upload process is started on the device in parallel. The download takes place using the `GetData` method in the predefined `AsyncOperation` delegate, so as not to wait for the video to load on the device. I tried running the data download in a separate `Task`, using `Task.Run ()`, but it didn't work. In general, this approach of downloading and playing videos does not claim to be the most effective, but I have not yet been able to come up with a better one. The file address is returned as a string, and it is used to play streaming videos using `VideoPlayer`.
 
> **Note:** For a video file to play successfully, it must match the streaming video format, and the web server you are connecting to must match the [**HLS**(HTTP Live Streaming)](https://en.wikipedia.org/wiki/HTTP_Live_Streaming) protocol.


### GetAssetBundle
This method is used for loading pre-prepared **AssetBundle** files. Takes as arguments a string with a url, `cancellationToken`, `Action` to display the progress, and a flag indicating whether to cache the downloaded audio file or not.
```csharp
public static async Task<AssetBundle> GetAssetBundle(string url, CancellationTokenSource cancelationToken, System.Action<float> progress = null, bool caching = true)
{
    UnityWebRequest request;
    CachedAssetBundle cachedAssetBundle = await GetCachedAssetBundle(url);
	
	if(caching && url.Contains("file://"))
    {
        caching = false; 
        Debug.LogWarning($"Caching of the file located at the {url} is rejected");
    }
	
    if (Caching.IsVersionCached(cachedAssetBundle) || (caching && ResourceCache.CheckFreeSpace(await GetSize(url))))
    {
        request = UnityWebRequestAssetBundle.GetAssetBundle(url, cachedAssetBundle, 0);
    }
    else
    {
        request = UnityWebRequestAssetBundle.GetAssetBundle(url)
    }

    UnityWebRequest uwr = await SendWebRequest(request, cancelationToken, Caching.IsVersionCached(cachedAssetBundle)? null : progress);
    if (uwr != null && !uwr.isHttpError && !uwr.isNetworkError)
    {
        AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
            if (caching) 
            {
                // Deleting old versions from the cache
                Caching.ClearOtherCachedVersions(assetBundle.name, cachedAssetBundle.hash);
            }
        return assetBundle;
    }
    else
    {
        throw new Exception("[Netowrk] error: " + uwr.error + " " + uwr.uri);
    }
}
```
First, the method checks for a previously cached version of **AssetBundle** using the auxiliary method `GetAssetBundleVersion(Uri uri)` provided below. If there is a cached version that matches the one stored at the specified link, then it is loaded. If there are no matches, the **AssetBundle** version is downloaded from the network. Кэшируется новая версия или нет, зависит от состояния флага `caching`. Whether the new version is cached or not depends on the state of the `caching` flag. This flag also determines whether previously downloaded versions will be deleted. The method returns an `AssetBundle` wrapped in a `Task`.

### GetAssetBundleVersion
An auxiliary method that helps determine the current version of **AssetBundle**. Accepts a string with a url as the only argument.
```csharp
private static async Task<CachedAssetBundle> GetAssetBundleVersion(string url)
{
    Hash128 hash = default;
    string localPath = new System.Uri(url).LocalPath;
    try
    {
        string manifest = await GetText(url + ".manifest");
        hash = GetHashFromManifest(manifest);
        return new CachedAssetBundle(localPath, hash);
    }
    catch (Exception e)
    {
        Debug.LogWarning("[Netowrk] error: " + e.Message);
        DirectoryInfo dir = new DirectoryInfo(url.ConvertToCachedPath());
        if (dir.Exists)
        {
            System.DateTime lastWriteTime = default;
            foreach (var item in dir.GetDirectories())
            {
                if (lastWriteTime < item.LastWriteTime)
                {
                    if (hash.isValid && hash != default) 
                    { 
                        Directory.Delete(Path.Combine(dir.FullName, hash.ToString()), true);
                    }
                    lastWriteTime = item.LastWriteTime;
                    hash = Hash128.Parse(item.Name);
                }
                else { Directory.Delete(Path.Combine(dir.FullName, item.Name), true); }
            }
            return new CachedAssetBundle(localPath, hash);
        }
        else
        {
            throw new Exception("[Netowrk] error: Nothing was found in the cache for " + url);
        }
    }
}
```
Before searching for the required up-to-date version of **AssetBundle** on the device, the method accesses the **AssetBundle** manifest file to extract the hash from it. If the hash is successfully loaded, the method packs it into the `CachedAssetBundle` structure and returns it. If the manifest failed to load, the method searches for the cached versions on the device and returns the hash packed in the `CachedAssetBundle` structure of the last one, if any.

I chose the `CachedAssetBundle` structure because, in addition to the hash, it contains the path to the **AssetBundle** file, which reduces the risk of collisions if the file names match.

> **Attention:** When caching files with the same name and local address, a collision will occur and only the last downloaded file will be cached. This situation may occur if only the domain names differ in the addresses of the uploaded files..

### GetHash128
A helper method that is used to extract the hash from the **AssetBundle** manifest.
Accepts a string with the url of the manifest file as the only argument.
```csharp
private static Hash128 GetHash128(this string str)
{
    var hashRow = str.Split("\n".ToCharArray())[5];
    var hash = Hash128.Parse(hashRow.Split(':')[1].Trim());
    if (hash.isValid && hash != default) { return hash; }
    else { throw new Exception("[Netowrk] error: couldn't extract hash from manifest."); }
}
```

### GetCachedPath
Method for getting the path to the cached file, if any.
Accepts a string with the file url as the only argument.
```csharp
private static async Task<string> GetCachedPath(this string url)
{
    string path = url.ConvertToCachedPath();
    if (File.Exists(path))
    {
        try
        {
            if (new FileInfo(path).Length != await GetSize(url)) { return url; }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Netowrk] error: " + e.Message); 
        }
        return "file://" + path;
    }
    else return url;
}
```
When the method is called, the url is converted to the path to the file, and if it is located there, its size is compared with the size of the file in the external storage. If the dimensions match, the method returns the path to the file as a string, if not, it returns the utl address.

Thank you for reading to the end, I hope this will be useful to you :)
