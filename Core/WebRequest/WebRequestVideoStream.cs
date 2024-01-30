namespace ru.ididdidi.Unity3D
{
    /// <summary>
    /// A specialized class for passing a link to a video stream as <see. cref="string"/>
    /// </summary>
    public class WebRequestVideoStream : WebRequest<string>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="url">URL address</param>
        public WebRequestVideoStream(string url) : base(url) { }

        /// <summary>
        /// Send a web request to the specified url.
        /// </summary>
        public override void Send()
        {
            Handler?.Invoke(url);
        }
    }
}