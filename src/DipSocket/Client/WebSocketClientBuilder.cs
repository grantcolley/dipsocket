namespace DipSocket.Client
{
    public class WebSocketClientBuilder
    {
        private WebSocketClient WebSocketConnection;

        private WebSocketClientBuilder()
        {
            WebSocketConnection = new WebSocketClient();
        }

        public WebSocketClient WithUrl(string url)
        {
            return WebSocketConnection;
        }

        public WebSocketClient Build()
        {
            return WebSocketConnection;
        }
    }
}