# dipsocket

A lightweight publisher / subscriber implementation using WebSockets.

[![Build status](https://ci.appveyor.com/api/projects/status/2v4p02f4xrav4oeq?svg=true)](https://ci.appveyor.com/project/grantcolley/dipsocket)

[NuGet package](https://www.nuget.org/packages/DipSocket/).

![Alt text](/README-images/dipsocket.png?raw=true "DipSocket")

#### Table of Contents
* [DipSocketServer](#dipsocketserver)
* [DipSocketClient](#dipsocketclient)
* [Example Usage](#example-usage)
  * [Client Connect](#client-connect)
  * [Client Message](#client-message)
  * [Server Implementation](#server-implementation)

## DipSocketServer
[DipSocketServer](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Server/DipSocketServer.cs) hosts and manages a collection of [DipSocketClient](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Client/DipSocketClient.cs) connections. It also manages a collection of Channels. A Channel is a group of client connections that subscribe to it. Messages from one client connection to another client or to a channel get routed via the server. When a client sends a message to a channel the message is broadcast to all connections subscribing to the channel.

## DipSocketClient
[DipSocketClient](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Client/DipSocketClient.cs) represents a client connection to the [DipSocketServer](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Server/DipSocketServer.cs). Client connections can send messages to each other, routed via the server. Client connections can also create and subscribe to channels hosted by the [DipSocketServer](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Server/DipSocketServer.cs).

## Example Usage

### Client Connect
[Full example here...](https://github.com/grantcolley/dipsocket/blob/master/test/Client/ViewModel/ChatViewModel.cs)
Establish a [DipSocketClient](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Client/DipSocketClient.cs) connection to the [DipSocketServer](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Server/DipSocketServer.cs).
```C#
                dipSocketClient = new DipSocketClient(@"ws://localhost:6000/chat", "clientId");
                dipSocketClient.Closed += DipSocketClientClosed; ;
                dipSocketClient.Error += DipSocketClientError;

                dipSocketClient.On("OnConnected", (result) =>
                {
                    OnConnected(result);
                });

                dipSocketClient.On("OnMessageReceived", (result) =>
                {
                    OnMessageReceived(result);
                });

                await dipSocketClient.StartAsync();
```

### Client Message
[Full example here...](https://github.com/grantcolley/dipsocket/blob/master/test/Client/ViewModel/ChatViewModel.cs)

Send a message from one [DipSocketClient](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Client/DipSocketClient.cs) to another. Messages are routed via the server.
```C#
                var clientMessage = new Message
                {
                    SenderConnectionId = senderConnectionId,
                    RecipientConnectionId = recipientConnectionId,
                    Data = myMessage,
                    MessageType = MessageType.SendToClient
                };
                
                await dipSocketClient.SendMessageAsync(clientMessage);
```

Send a message from a [DipSocketClient](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Client/DipSocketClient.cs) to channel on the [DipSocketServer](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Server/DipSocketServer.cs).
```C#
                var clientMessage = new Message
                {
                    SenderConnectionId = senderConnectionId,
                    RecipientConnectionId = channelConnectionId,
                    Data = myMessage,
                    MessageType = MessageType.SendToChannel
                };
                
                await dipSocketClient.SendMessageAsync(clientMessage);
```

### Server Implementation
Inheriting abstract [DipSocketServer](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Server/DipSocketServer.cs).
```C#

```
