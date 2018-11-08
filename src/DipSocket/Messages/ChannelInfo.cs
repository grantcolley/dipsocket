using System.Collections.Generic;

namespace DipSocket.Messages
{
    public class ChannelInfo
    {
        public ChannelInfo()
        {
            Connections = new List<string>();
        }

        public string Name { get; set; }
        public List<string> Connections { get; set; }
    }
}
