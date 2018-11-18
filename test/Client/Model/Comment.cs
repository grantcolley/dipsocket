using System;

namespace Client.Model
{
    public class Comment
    {
        public DateTime SentOn { get; set; }
        public string Sender { get; set; }
        public string Text { get; set; }
    }
}
