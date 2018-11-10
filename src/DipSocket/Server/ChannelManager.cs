﻿using DipSocket.Messages;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        internal List<Channel> GetChannels()
        {
            return channels.Values.ToList();
        }

        internal List<ChannelInfo> GetChannelInfos()
        {
            return channels.Values.Select(c => c.GetChannelInfo()).ToList();
        }

        internal Channel SubscribeToChannel(string channelName, Connection connection)
        {
            var channel = channels.GetOrAdd(channelName, name =>
            {
                return new Channel { Name = name};
            });

            if (channel.Connections.TryAdd(connection.ConnectionId, connection))
            {
                return channel;
            }

            return null;
        }

        internal Channel UnsubscribeFromChannel(string channelName, Connection connection)
        {
            if (!channels.ContainsKey(channelName))
            {
                return null;
            }

            if (channels.TryGetValue(channelName, out Channel channel))
            {
                channel.Connections.TryRemove(connection.ConnectionId, out Connection removedConnection);

                if (channel.Connections.Any())
                {
                    return channel;
                }

                if (TryRemoveChannel(channelName, out Channel removedChannel))
                {
                    return removedChannel;
                }
            }

            return null;
        }

        internal Channel GetChannel(string channelName)
        {
            if(channels.TryGetValue(channelName, out Channel channel))
            {
                return channel;
            }

            return null;
        }

        internal bool TryRemoveChannel(string channelName, out Channel channel)
        {
            return channels.TryRemove(channelName, out channel);
        }
    }
}