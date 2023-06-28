using Google.Cloud.Firestore;

namespace Server.Models
{
    [FirestoreData]
    public class MessageOut
    {
        [FirestoreProperty]
        public string SenderID { get; set; }

        [FirestoreProperty]
        public string Content { get; set; }

        [FirestoreProperty]
        public object CreateTime { get; set; }
    }
}
