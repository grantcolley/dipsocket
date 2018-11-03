using System.Collections.Generic;

namespace DipSocket
{
    public class Channel : Connection
    {
        public Channel()
        {
            Connections = new List<Connection>();
        }

        public List<Connection> Connections { get; set; }
    }
}
