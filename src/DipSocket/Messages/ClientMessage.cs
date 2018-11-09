using DipSocket.Client;
using DipSocket.Server;

namespace DipSocket.Messages
{
    /// <summary>
    /// A message from the <see cref="DipSocketClient"/> to the <see cref="DipSocketServer"/>.
    /// </summary>
    public class ClientMessage : Message
    {
        /// <summary>
        /// Gets or sets the recipient of the message which is either another <see cref="Connection"/> or a <see cref="Channel"/>.
        /// </summary>
        public string SendTo { get; set; }

        /// <summary>
        /// Gets or sets the type of message from the <see cref="DipSocketClient"/> to the <see cref="DipSocketServer"/>.
        /// </summary>
        public MessageType MessageType { get; set; }
    }
}
