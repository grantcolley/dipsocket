using DipSocket.Messages;
using System.Collections.ObjectModel;

namespace Client.Model
{
    public abstract class InfoDecorator : NotifyPropertyChangedBase, IInfo
    {
        protected IInfo info;

        protected InfoDecorator(IInfo info)
        {
            this.info = info;

            Conversation = new ObservableCollection<Comment>();
        }

        public ObservableCollection<Comment> Conversation { get; }

        public string Name
        {
            get { return info.Name; }
            set
            {
                if (info != null
                    && info.Name != value)
                {
                    info.Name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string ConnectionId
        {
            get { return info.ConnectionId; }
            set
            {
                if (info != null
                    && info.ConnectionId != value)
                {
                    info.ConnectionId = value;
                    OnPropertyChanged("ConnectionId");
                }
            }
        }

    }
}