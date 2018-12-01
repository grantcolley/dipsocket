using DipSocket.Messages;
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
        }
    }
}
