using Google.Cloud.Firestore;

namespace Server.Models
{
    [FirestoreData]
    public class Message
    {
        [FirestoreDocumentId]
        public string MessageID { get; set; }

        [FirestoreProperty]
        public string SenderID { get; set; }

        [FirestoreProperty]
        public string Content { get; set; }

        [FirestoreDocumentCreateTimestamp]
        public DateTime CreateTime { get; set; }
    }
}
