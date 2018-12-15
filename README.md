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

## DipSocketServer
DipSocketServer hosts and manages a collection of DipSocketClient connections. It also manages a collection of Channels. A Channel is a group of client connections that subscribe to it. Messages from one client connection to another client or to a channel get routed via the server. When a client sends a message to a channel the message is broadcast to all connections subscribing to the channel.

## DipSocketClient
DipSocketClient represents a client connection to the DipSocketServer. Client connections can send messages to each other, routed via the server. Client connections can also create and subscribe to channels hosted by the DipSocketServer.

## Example Usage

### Client Connect
Establish a DipSocketClient connection to the DipSocketServer.
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
Send a message from one DipSocketClient to another. Messages are routed via the server.
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

Send a message from a DipSocketClient to channel on the DipSocketServer.
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
