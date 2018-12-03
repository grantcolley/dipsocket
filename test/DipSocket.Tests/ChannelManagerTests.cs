﻿using DipSocket.Messages;
using DipSocket.Server;
using System.Linq;
using System.Net.WebSockets;
using Xunit;

namespace DipSocket.Tests
{
    public class ChannelManagerTests
    {
        [Fact]
        public void SubscribeToChannel()
        {
            // Arrange
            var channelManager = new ChannelManager();
            var connection1 = new Connection(new ClientWebSocket()) { ConnectionId = "123", Name = "conn1" };
            var connection2 = new Connection(new ClientWebSocket()) { ConnectionId = "456", Name = "conn2" };

            // Act
            var channelResult1 = channelManager.SubscribeToChannel("channel1", connection1);
            var channelResult2 = channelManager.SubscribeToChannel("channel1", connection2);

            // Assert
            Assert.Equal(channelResult1, channelResult2);
            Assert.Equal("channel1", channelResult1.Name);
            Assert.Equal(2, channelResult1.Connections.Count);
            Assert.Contains(connection1.ConnectionId, channelResult1.Connections.Keys);
            Assert.Contains(connection2.ConnectionId, channelResult1.Connections.Keys);
            Assert.Contains(channelResult1.Name, connection1.Channels.Keys);
            Assert.Contains(channelResult2.Name, connection2.Channels.Keys);
        }

        [Fact]
        public void GetChannel()
        {
            // Arrange
            var channelManager = new ChannelManager();
            var connection = new Connection(new ClientWebSocket()) { ConnectionId = "123", Name = "conn" };
            var channel = channelManager.SubscribeToChannel("channel", connection);

            // Act
            var result = channelManager.GetChannel(channel.Name);

            // Assert
            Assert.Equal(channel, result);
        }

        [Fact]
        public void GetChannel_NotFound()
        {
            // Arrange
            var channelManager = new ChannelManager();
            var connection = new Connection(new ClientWebSocket()) { ConnectionId = "123", Name = "conn" };
            var channel = channelManager.SubscribeToChannel("channel", connection);

            // Act
            var result = channelManager.GetChannel("ABC");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetChannels()
        {
            // Arrange
            var channelManager = new ChannelManager();
            var connection1 = new Connection(new ClientWebSocket()) { ConnectionId = "123", Name = "conn1" };
            var connection2 = new Connection(new ClientWebSocket()) { ConnectionId = "456", Name = "conn2" };
            var channel1 = channelManager.SubscribeToChannel("channel1", connection1);
            var channel2 = channelManager.SubscribeToChannel("channel2", connection2);

            // Act
            var channels = channelManager.GetChannels();

            // Assert
            Assert.Equal(2, channels.Count);
            Assert.Contains(channel1, channels);
            Assert.Contains(channel2, channels);
        }

        [Fact]
        public void GetChannelInfos()
        {
            // Arrange
            var channelManager = new ChannelManager();
            var connection1 = new Connection(new ClientWebSocket()) { ConnectionId = "123", Name = "conn1" };
            var connection2 = new Connection(new ClientWebSocket()) { ConnectionId = "456", Name = "conn2" };
            var channel1 = channelManager.SubscribeToChannel("channel1", connection1);
            var channel2 = channelManager.SubscribeToChannel("channel2", connection2);

            // Act
            var channelInfos = channelManager.GetChannelInfos();

            // Assert
            Assert.Equal(2, channelInfos.Count);
            Assert.Single<ChannelInfo>(channelInfos, ci => { return ci.Name.Equals("channel1"); });
            Assert.Single<ChannelInfo>(channelInfos, ci => { return ci.Name.Equals("channel2"); });
        }

        [Fact]
        public void TryRemoveChannel_NotFound()
        {
            // Arrange
            var channelManager = new ChannelManager();
            var connection = new Connection(new ClientWebSocket()) { ConnectionId = "123", Name = "conn" };
            var channel = channelManager.SubscribeToChannel("channel", connection);

            // Act
            var result = channelManager.TryRemoveChannel("ABC", out Channel notFound);

            // Assert
            Assert.False(result);
            Assert.Null(notFound);
        }

        [Fact]
        public void TryRemoveChannel()
        {
            // Arrange
            var channelManager = new ChannelManager();
            var connection1 = new Connection(new ClientWebSocket()) { ConnectionId = "123", Name = "conn1" };
            var connection2 = new Connection(new ClientWebSocket()) { ConnectionId = "456", Name = "conn2" };
            var channelResult1 = channelManager.SubscribeToChannel("channel1", connection1);
            var channelResult2 = channelManager.SubscribeToChannel("channel1", connection2);

            // Act
            var result = channelManager.TryRemoveChannel("channel1", out Channel channel);

            // Assert
            Assert.True(result);
            Assert.Equal("channel1", channel.Name);
            Assert.Empty(channel.Connections);
            Assert.Empty(connection1.Channels);
            Assert.Empty(connection2.Channels);
        }

        [Fact]
        public void UnsubscribeFromChannel()
        {

        }
    }
}
