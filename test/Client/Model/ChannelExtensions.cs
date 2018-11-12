using DipSocket.Messages;
using System.Linq;

namespace Client.Model
{
    public static class ChannelExtensions
    {
        public static Channel UpdateConnections(this Channel channel, ChannelInfo channelInfo)
        {
            var removes = channel.Connections.Where(c => !channelInfo.Connections.Any(ci => ci.Name.Equals(c.Name))).ToList();
            foreach(var remove in removes)
            {
                channel.Connections.Remove(remove);
            }

            var additions = channelInfo.Connections.Where(ci => !channel.Connections.Any(c => c.Name.Equals(ci.Name))).ToList();
            foreach(var addition in additions)
            {
                channel.Connections.Add(addition);
            }
            
            return channel;
        }
    }
}
