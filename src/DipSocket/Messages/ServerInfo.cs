using System.Collections.Generic;

namespace DipSocket.Messages
{
    public class ServerInfo
    {
        public List<ConnectionInfo> Connections { get; set; }
        public List<ChannelInfo> Channels { get; set; }
    }
}
