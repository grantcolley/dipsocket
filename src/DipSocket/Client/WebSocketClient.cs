using System;
using System.Threading;
using System.Threading.Tasks;

namespace DipSocket.Client
{
    public class WebSocketClient
    {
        public event Func<Exception, Task> Closed;

        public Task DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public IDisposable On(string methodName, Action<object> handler)
        {
            throw new NotImplementedException();
        }

        public Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        private void OnClose(Exception exception)
        {
            var closed = Closed;
            closed.Invoke(exception);
        }
    }
}
