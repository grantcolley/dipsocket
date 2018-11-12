using DipSocket.Messages;

namespace Client.Model
{
    public class Connection : InfoDecorator
    {
        public Connection(ConnectionInfo connectionInfo) : base(connectionInfo) { }
    }
}
