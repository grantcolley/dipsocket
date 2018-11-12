using DipSocket.Messages;
using System.Collections.ObjectModel;

namespace Client.Model
{
    public class Channel : InfoDecorator
    {
        public ObservableCollection<ConnectionInfo> Connections { get; }

        public Channel(ChannelInfo channelInfo) : base(channelInfo)
        {
            Connections = new ObservableCollection<ConnectionInfo>(channelInfo.Connections);
            channelInfo.Connections.Clear();
        }
    }
}