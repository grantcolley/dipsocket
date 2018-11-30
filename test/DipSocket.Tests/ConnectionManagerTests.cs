using DipSocket.Messages;
using DipSocket.Server;
using System.Linq;
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
        public void GetConnectionId_NotFound()
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
        public void GetConnection_WebSocket()
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
        public void GetConnection_WebSocket_NotFound()
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
        public void GetConnection_ConnectionId()
        {
            // Arrange
            var connectionManager = new ConnectionManager();
            var webSocket = new ClientWebSocket();
            var result = connectionManager.TryAddWebSocketConnection(webSocket, out Connection connection);

            // Act
            var conn = connectionManager.GetConnection(connection.ConnectionId);

            // Assert
            Assert.Equal(connection, conn);
        }

        [Fact]
        public void GetConnection_ConnectionId_NotFound()
        {
            // Arrange
            var connectionManager = new ConnectionManager();
            var webSocket = new ClientWebSocket();
            var result = connectionManager.TryAddWebSocketConnection(webSocket, out Connection connection);

            // Act
            var conn = connectionManager.GetConnection("123");

            // Assert
            Assert.Null(conn);
        }

        [Fact]
        public void GetConnections()
        {
            // Arrange
            var connectionManager = new ConnectionManager();
            var webSocket1 = new ClientWebSocket();
            var webSocket2 = new ClientWebSocket();
            var result1 = connectionManager.TryAddWebSocketConnection(webSocket1, out Connection connection1);
            var result2 = connectionManager.TryAddWebSocketConnection(webSocket2, out Connection connection2);

            // Act
            var connections = connectionManager.GetConnections();

            // Assert
            Assert.Equal(2, connections.Count());
            Assert.Equal(connection1, connections[0]);
            Assert.Equal(connection2, connections[1]);
        }

        [Fact]
        public void GetConnectionInfos()
        {
            // Arrange
            var connectionManager = new ConnectionManager();
            var webSocket1 = new ClientWebSocket();
            var webSocket2 = new ClientWebSocket();
            var result1 = connectionManager.TryAddWebSocketConnection(webSocket1, out Connection connection1);
            var result2 = connectionManager.TryAddWebSocketConnection(webSocket2, out Connection connection2);

            // Act
            var connectionInfos = connectionManager.GetConnectionInfos();

            // Assert
            Assert.Equal(2, connectionInfos.Count());
            Assert.Equal(connection1.GetConnectionInfo().ConnectionId, connectionInfos[0].ConnectionId);
            Assert.Equal(connection2.GetConnectionInfo().ConnectionId, connectionInfos[1].ConnectionId);
        }

        [Fact]
        public void TryRemoveWebSocketConnection()
        {
            // Arrange
            var connectionManager = new ConnectionManager();
            var webSocket1 = new ClientWebSocket();
            var webSocket2 = new ClientWebSocket();
            var result1 = connectionManager.TryAddWebSocketConnection(webSocket1, out Connection connection1);
            var result2 = connectionManager.TryAddWebSocketConnection(webSocket2, out Connection connection2);

            // Act
            var result = connectionManager.TryRemoveWebSocketConnection(webSocket1, out Connection connection);

            // Assert
            var connections = connectionManager.GetConnections();
            Assert.Single(connections);
            Assert.Equal(connection2, connections[0]);
            Assert.Equal(connection1, connection);
        }
    }
}
