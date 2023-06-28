using Google.Cloud.Firestore;

namespace Server.Models
{
    [FirestoreData]
    public class User
    {
        [FirestoreDocumentId]
        public string UserID { get; set; }

        [FirestoreProperty]
        public string Email { get; set; }

        [FirestoreProperty]
        public DateTime LastOnlineTime { get; set; }

    }
}
