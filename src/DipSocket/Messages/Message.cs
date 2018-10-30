using System;

namespace DipSocket.Messages
{
    public abstract class Message
    {
        protected Message()
        {
            SentOn = DateTime.Now;
        }

        public DateTime SentOn { get; set; }
        public string SentBy { get; set; }
        public string Data { get; set; }
    }
}
