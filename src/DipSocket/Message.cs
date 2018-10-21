using System;

namespace DipSocket
{
    public class Message
    {
        public Message()
        {
            SentOn = DateTime.Now;
        }

        public string MethodName { get; set; }
        public DateTime SentOn { get; set; }
        public string SentBy { get; set; }
        public string Data { get; set; }
    }
}
