﻿namespace ru.ididdidi.Unity3D
{
    /// <summary>
    /// Interface for interacting with a web request.
    /// </summary>
    public interface IWebRequest
    {
        /// <summary>
        /// URL address.
        /// </summary>
        string url { get; set; }
        /// <summary>
        /// Send a web request to the specified url.
        /// </summary>
        void Send();
        /// <summary>
        /// Cancel web request.
        /// </summary>
        void Cancel();
    }
}