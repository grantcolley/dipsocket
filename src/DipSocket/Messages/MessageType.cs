namespace DipSocket.Messages
{
    /// <summary>
    /// Indicates the type of message being sent from a <see cref="DipSocketClient"/> to the <see cref="DipSocketServer"/>.
    /// </summary>
    public enum MessageType
    {
        CreateNewChannel,
        Disconnect,
        SendToAll,
        SendToChannel,
        SendToClient,
        SubscribeToChannel,
        UnsubscribeFromChannel
    }
}