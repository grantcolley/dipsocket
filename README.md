# dipsocket

A lightweight WebSocket publisher / subscriber library.

[![Build status](https://ci.appveyor.com/api/projects/status/2v4p02f4xrav4oeq?svg=true)](https://ci.appveyor.com/project/grantcolley/dipsocket)

[NuGet package](https://www.nuget.org/packages/DipSocket/).

Publisher subcriber wrapper using WebSocket's.

#### Table of Contents
* [Example Usage](#example-usage)
  * [Client Connect](#client-connect-usage)
  * [Client Message](#client-message-usage)

## Example Usage

### Client Connect
Establish a client connection to the server.
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
Send a message from one client cocket to another
```C#
                var clientMessage = new Message
                {
                    SenderConnectionId = User.ConnectionId,
                    RecipientConnectionId = SelectedInfo.ConnectionId,
                    Data = Message,
                    MessageType = MessageType.SendToClient
                };
                
                await dipSocketClient.SendMessageAsync(clientMessage);
```
