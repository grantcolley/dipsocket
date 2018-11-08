namespace DipSocket.Messages
{
    public enum MessageType
    {
        CreateNewChannel,
        SendToAll,
        SendToChannel,
        SendToClient,
        SubscribeToChannel,
        UnsubscribeFromChannel
    }
}