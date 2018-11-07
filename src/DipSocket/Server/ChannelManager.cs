using System.Collections.Concurrent;
using System.Linq;

namespace DipSocket.Server
{
    /// <summary>
    /// The <see cref="ChannelManager"/> class stores and manages access to a 
    /// <see cref="ConcurrentDictionary{string, Channel}"/> of <see cref="Channel"/>'s 
    /// for client <see cref="WebSocket"/>'s to subscribe to.
    /// </summary>
    public sealed class ChannelManager
    {
        private ConcurrentDictionary<string, Channel> channels;

        /// <summary>
        /// Creates a new instance of the <see cref="ChannelManager"/> class.
        /// </summary>
        public ChannelManager()
        {
            channels = new ConcurrentDictionary<string, Channel>();
        }

        internal Channel SubscribeToChannel(string channelName, Connection connection)
        {
            var channel = channels.GetOrAdd(channelName, name =>
            {
                return new Channel { Name = name};
            });

            channel.Connections.Add(connection);

            return channel;
        }

        internal Channel UnsubscribeFromChannel(string channelName, Connection connection)
        {
            if (!channels.ContainsKey(channelName))
            {
                return null;
            }

            var result = channels.TryGetValue(channelName, out Channel channel);

            channel.Connections.Remove(connection);

            if (!channel.Connections.Any())
            {
                return channel;
            }

            channels.TryRemove(channelName, out Channel removedChannel);
            return removedChannel;
        }

        internal Channel GetChannel(string channelName)
        {
            if(channels.TryGetValue(channelName, out Channel channel))
            {
                return channel;
            }

            return null;
        }

        internal bool TryRemoveChannel(string channelName)
        {
            var result = channels.TryRemove(channelName, out Channel channel);
            channel?.Connections.Clear();
            return result;
        }
    }
}