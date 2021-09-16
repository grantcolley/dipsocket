# dipsocket

A lightweight publisher / subscriber implementation using WebSockets.

*.NET Standard 2.0*, *.NET Core 2.1* and *ASP .NET Core*

[![Build status](https://ci.appveyor.com/api/projects/status/2v4p02f4xrav4oeq?svg=true)](https://ci.appveyor.com/project/grantcolley/dipsocket)

NuGet
* [DipSocket](https://www.nuget.org/packages/DipSocket/).
* [DipSocket.NetCore.Extensions](https://www.nuget.org/packages/DipSocket.netcore.extensions/).
<br/>
<br/>

Example of ***dipsocket*** being used as a chat application where users connect to a channel and join the discussion.

![Alt text](/README-images/chat.png?raw=true "Chat")

#### Table of Contents
* [DipSocketServer](#dipsocketserver)
* [DipSocketClient](#dipsocketclient)
* [DipSocket NetCore Extensions](#dipsocket-netcore-extensions)
* [Example Usage](#example-usage)
  * [Client Connect](#client-connect)
  * [Client Message](#client-message)
  * [Server Implementation](#server-implementation)
  * [Server Configure and Build](#server-configure-and-build)
* [Running the Example](#running-the-example)

## DipSocketServer
*.NET Standard 2.0*

[DipSocketServer](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Server/DipSocketServer.cs) hosts and manages a collection of [DipSocketClient](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Client/DipSocketClient.cs) connections. It also manages a collection of Channels. A Channel is a group of client connections that subscribe to it. Messages from one client connection to another client or to a channel get routed via the server. When a client sends a message to a channel the message is broadcast to all connections subscribing to the channel.

## DipSocketClient
*.NET Standard 2.0*

[DipSocketClient](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Client/DipSocketClient.cs) represents a client connection to the [DipSocketServer](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Server/DipSocketServer.cs). Client connections can send messages to each other, routed via the server. Client connections can also create and subscribe to channels hosted by the [DipSocketServer](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Server/DipSocketServer.cs).

## DipSocket NetCore Extensions
*.NET Core 2.1* and *ASP NET Core*

[DipSocket.NetCore.Extensions.dll](https://github.com/grantcolley/dipsocket/tree/master/src/DipSocket.NetCore.Extensions) provides the middleware and extension methods necessary for ASP .NET Core. Simply add dip socket to the services collection and get the app builder to use it.

## Example Usage

### Client Connect
[Full example here...](https://github.com/grantcolley/dipsocket/blob/master/test/Client/ViewModel/ChatViewModel.cs)

Establish a [DipSocketClient](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Client/DipSocketClient.cs) connection to the [DipSocketServer](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Server/DipSocketServer.cs).
```C#
                var dipSocketClient = new DipSocketClient(@"ws://localhost:6000/chat", "clientId");
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

Send a message from a [DipSocketClient](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Client/DipSocketClient.cs) to a channel on the [DipSocketServer](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Server/DipSocketServer.cs). The message is broadcast to all clients subscribing to the channel. 
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
[Full example here...](https://github.com/grantcolley/dipsocket/blob/master/test/Server/Chat.cs)

Inherit the abstract [DipSocketServer](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Server/DipSocketServer.cs) class and override abstract methods *OnClientConnectAsync* and *ReceiveAsync*.
```C#
    public class Chat : DipSocketServer
    {
        public async override Task OnClientConnectAsync(WebSocket websocket, string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException("clientId cannot be null or empty.");
            }

            var connection = await base.AddWebSocketAsync(websocket).ConfigureAwait(false);

            connection.Name = clientId;

            var connectionInfo = connection.GetConnectionInfo();

            var json = JsonConvert.SerializeObject(connectionInfo);
            
            var message = new Message { MethodName = "OnConnected", SenderConnectionId = "Chat", Data = json };

            await SendMessageAsync(websocket, message).ConfigureAwait(false);
        }

        public async override Task ReceiveAsync(WebSocket webSocket, Message message)
        {
            switch (message.MessageType)
            {
                case MessageType.SendToAll:
                    message.MethodName = "OnMessageReceived";
                    await SendMessageToAllAsync(message).ConfigureAwait(false);
                    break;

                case MessageType.SendToClient:
                    message.MethodName = "OnMessageReceived";
                    await SendMessageAsync(message).ConfigureAwait(false);
                    break;

                case MessageType.SubscribeToChannel:
                    var channel = SubscribeToChannel(message.Data, webSocket);
                    await ChannelUpdateAsync().ConfigureAwait(false);
                    break;
            }
        }
    }
```

### Server Configure and Build
In the *Startup* class use the *IServiceCollection* extension method [*AddDipSocket*](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket.NetCore.Extensions/ServiceCollectionExtensions.cs) and the *IApplicationBuilder* extension method [*UseDipSocket\<T>*](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket.NetCore.Extensions/MiddlewareExtensions.cs). This will register each implementation of the *[DipSocketServer](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Server/DipSocketServer.cs)* as a singleton in the service collection and add the [middleware](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket.NetCore.Extensions/DipSocketMiddleware.cs).

The following shows how to set up the [*Chat*](https://github.com/grantcolley/dipsocket/blob/master/test/Server/Chat.cs) implementation of the *[DipSocketServer](https://github.com/grantcolley/dipsocket/blob/master/src/DipSocket/Server/DipSocketServer.cs)* in the example code.
 
```C#
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDipSocket();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseDipSocket<Chat>("/chat");
        }
    }
```

## Running the Example
The example solution provided is a simple chat application. It consists of a WPF [client](https://github.com/grantcolley/dipsocket/tree/master/test/Client) and a ASP .NET Core [server](https://github.com/grantcolley/dipsocket/tree/master/test/Server).

First launch the server. Then run multiple instances of the *Client.exe*.

For each instance of the client provide a unique user name:

![Alt text](/README-images/login.png?raw=true "Login")

The client will automatically attempt to connect to the server. 

Either add a new channel, or select an existing connection or channel from the drop down list:

![Alt text](/README-images/connect.png?raw=true "Connect")

Write your message in the text box at the bottom and hit enter:

![Alt text](/README-images/chat.png?raw=true "Chat")
