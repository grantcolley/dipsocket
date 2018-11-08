using System.Collections.Concurrent;

namespace DipSocket.Server
{
    public sealed class Channel
    {
        internal Channel()
        {
            Connections = new ConcurrentDictionary<string, Connection>();
        }

        public string Name { get; set; }
        public ConcurrentDictionary<string, Connection> Connections { get; set; }
    }
}
