using DipSocket.Messages;

namespace Client.Model
{
    public class InfoFactory
    {
        public static IInfo GetInfo(IInfo info)
        {
            if(info is ChannelInfo channelInfo)
            {
                return new Channel(channelInfo);
            }

            if(info is ConnectionInfo connectionInfo)
            {
                return new Connection(connectionInfo);
            }

            return null;
        }
    }
}