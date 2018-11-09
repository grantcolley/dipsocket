namespace DipSocket.Server
{
    /// <summary>
    /// Represents information about an active <see cref="Connection"/> <see cref="DipSocketServer"/>.
    /// </summary>
    public class ConnectionInfo
    {
        /// <summary>
        /// Gets or sets the name of the connection.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the connection id;
        /// </summary>
        public string ConnectionId { get; set; }
    }
}