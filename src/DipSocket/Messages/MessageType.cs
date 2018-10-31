namespace DipSocket.Messages
{
    public enum MessageType
    {
        SendToAll,
        SendToClient,
        CreateNewChannel,
        SubscribeToChannel,
        SendToChannel,
        UnsubscribeFromChannel
    }
}