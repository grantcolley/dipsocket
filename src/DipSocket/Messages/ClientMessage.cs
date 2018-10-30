namespace DipSocket.Messages
{
    public class ClientMessage : Message
    {
        public string SendTo { get; set; }
        public MessageType MessageType { get; set; }
    }
}
