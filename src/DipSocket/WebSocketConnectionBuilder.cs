namespace DipSocket
{
    public class WebSocketConnectionBuilder
    {
        private WebSocketConnection WebSocketConnection;

        private WebSocketConnectionBuilder()
        {
            WebSocketConnection = new WebSocketConnection();
        }

        public WebSocketConnection WithUrl(string url)
        {
            return WebSocketConnection;
        }

        public WebSocketConnection Build()
        {
            return WebSocketConnection;
        }
    }
}