using System.Collections.Generic;

namespace DipSocket.Messages
{
    public class ServerConnections
    {
        public List<Connection> Connections { get; set; }
        public List<Channel> Channels { get; set; }
    }
}
