using System.Collections.Generic;

namespace DipSocket
{
    public class Connection
    {
        public Connection()
        {
            Channels = new List<Channel>();
        }

        public string Name { get; set; }
        public string ConnectionId { get; set; }
        public List<Channel> Channels { get; set; }
    }
}