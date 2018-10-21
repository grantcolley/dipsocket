using System;

namespace Chat
{
    public class ChatMessage
    {
        public ChatMessage()
        {
            SentOn = DateTime.Now;
        }

        public string MethodName { get; set; }
        public DateTime SentOn { get; set; }
        public string SentBy { get; set; }
        public string Text { get; set; }
    }
}
