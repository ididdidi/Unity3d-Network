namespace ru.ididdidi.Unity3D
{
    public class WebRequestVideoStream : WebRequest<string>
    {
        public WebRequestVideoStream(string url) : base(url) { }

        public override void Send()
        {
            Handler?.Invoke(url);
        }
    }
}