using System.Collections.Generic;

namespace DipSocket
{
    public class Channel
    {
        public Channel()
        {
            Connections = new List<Connection>();
        }

        public string Name { get; set; }
        public List<Connection> Connections { get; set; }
    }
}
