using DipSocket.Server;
using System.Net.WebSockets;
using Xunit;

namespace TestCaases
{
    public class ConnectionManagerTests
    {
        [Fact]
        public void TryAddWebSocketConnection()
        {
            // Arrange
            var connectionManager = new ConnectionManager();
            var webSocket = new ClientWebSocket();

            // Act
            var result = connectionManager.TryAddWebSocketConnection(webSocket, out Connection connection);

            // Assert
            Assert.True(result);
            Assert.NotNull(connection);
        }

        [Fact]
        public void GetConnectionId()
        {
            // Arrange
            var connectionManager = new ConnectionManager();
            var webSocket = new ClientWebSocket();
            var result = connectionManager.TryAddWebSocketConnection(webSocket, out Connection connection);

            // Act
            var connectionId = connectionManager.GetConnectionId(webSocket);

            // Assert
            Assert.Equal(connection.ConnectionId, connectionId);
        }

        [Fact]
        public void GetConnectionId_Fails()
        {
            // Arrange
            var connectionManager = new ConnectionManager();
            var webSocket1 = new ClientWebSocket();
            var webSocket2 = new ClientWebSocket();
            var result = connectionManager.TryAddWebSocketConnection(webSocket1, out Connection connection);

            // Act
            var connectionId = connectionManager.GetConnectionId(webSocket2);

            // Assert
            Assert.Null(connectionId);
        }

        [Fact]
        public void GetConnection()
        {
            // Arrange
            var connectionManager = new ConnectionManager();
            var webSocket = new ClientWebSocket();
            var result = connectionManager.TryAddWebSocketConnection(webSocket, out Connection connection);

            // Act
            var conn = connectionManager.GetConnection(webSocket);

            // Assert
            Assert.Equal(connection, conn);
        }

        [Fact]
        public void GetConnection_Fails()
        {
            // Arrange
            var connectionManager = new ConnectionManager();
            var webSocket1 = new ClientWebSocket();
            var webSocket2 = new ClientWebSocket();
            var result = connectionManager.TryAddWebSocketConnection(webSocket1, out Connection connection);

            // Act
            var conn = connectionManager.GetConnection(webSocket2);

            // Assert
            Assert.Null(conn);
        }

        [Fact]
        public void TryRemoveWebSocketConnection()
        {
            // Arrange

            // Act

            // Assert
        }
    }
}
