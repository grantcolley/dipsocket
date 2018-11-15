namespace DipSocket.Messages
{
    /// <summary>
    /// Indicates the type of message being sent.
    /// </summary>
    public enum MessageType
    {
        Disconnect,
        SendToAll,
        SendToChannel,
        SendToClient,
        SubscribeToChannel,
        UnsubscribeFromChannel
    }
}