using Google.Cloud.Firestore;

namespace Server.Models
{
    [FirestoreData]
    public class Friendship
    {
        [FirestoreDocumentId]
        public string FriendshipID { get; set; }

        [FirestoreProperty]
        public string User1ID { get; set; }

        [FirestoreProperty]
        public string User2ID { get; set; }
    }
}
