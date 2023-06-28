
using System;

namespace SDK.Models
{
    public class Message : IEquatable<Message>
    {
        public string MessageID { get; set; }
        public string SenderID { get; set; }
        public string Content { get; set; }
        public DateTime CreateTime { get; set; }

        public bool Equals(Message other) => MessageID == other.MessageID;
        public override bool Equals(object obj) => Equals(obj as Message);
        public override int GetHashCode() => (MessageID, SenderID).GetHashCode();
    }
}
