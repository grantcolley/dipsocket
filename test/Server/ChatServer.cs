using System.Net.WebSockets;
using System.Threading.Tasks;
using DipSocket;

namespace Server
{
    public class ChatServer : WebSocketServer
    {
        public override Task ReceiveAsync(WebSocket webSocket, WebSocketReceiveResult webSocketReceiveResult, byte[] buffer)
        {
            throw new System.NotImplementedException();
        }
    }
}
