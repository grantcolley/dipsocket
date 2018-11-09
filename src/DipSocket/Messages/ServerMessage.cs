using DipSocket.Client;
using DipSocket.Server;

namespace DipSocket.Messages
{
    /// <summary>
    /// A message from the <see cref="DipSocketServer"/> to the <see cref="DipSocketClient"/>.
    /// </summary>
    public class ServerMessage : Message
    {
        /// <summary>
        /// The name of the method to invoke on the client.
        /// </summary>
        public string MethodName { get; set; }
    }
}
