using System;

namespace DipSocket.Messages
{
    /// <summary>
    /// Base message class.
    /// </summary>
    public abstract class Message
    {
        /// <summary>
        /// Creates an instance of the base message.
        /// </summary>
        protected Message()
        {
            SentOn = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets the date / time the message is sent.
        /// </summary>
        public DateTime SentOn { get; set; }

        /// <summary>
        /// Gets or sets the senders name.
        /// </summary>
        public string SentBy { get; set; }

        /// <summary>
        /// Gets or sets the serialised message data.
        /// </summary>
        public string Data { get; set; }
    }
}
