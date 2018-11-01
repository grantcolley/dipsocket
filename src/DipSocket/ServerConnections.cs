using System.Collections.Generic;

namespace DipSocket
{
    public class ServerConnections
    {
        public ServerConnections()
        {
            ClientConnections = new List<ClientConnection>();
            Channels = new List<KeyValuePair<string, List<ClientConnection>>>();
        }

        public List<ClientConnection> ClientConnections { get; set; }
        public List<KeyValuePair<string, List<ClientConnection>>> Channels { get; set; }
    }
}
