# dipsocket

A lightweight publisher / subscriber implementation using WebSockets.

[![Build status](https://ci.appveyor.com/api/projects/status/2v4p02f4xrav4oeq?svg=true)](https://ci.appveyor.com/project/grantcolley/dipsocket)

[NuGet package](https://www.nuget.org/packages/DipSocket/).

![Alt text](/README-images/dipsocket.png?raw=true "DipSocket")

#### Table of Contents
* [Example Usage](#example-usage)
  * [Client Connect](#client-connect)
  * [Client Message](#client-message)

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
Send a message from a DipSocketClient to another via the DipSocketServer.
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

A channel is a collection of DipSocketClient connections. Send a message from a DipSocketClient to channel on the DipSocketServer.
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
