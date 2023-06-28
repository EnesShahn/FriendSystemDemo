using Google.Cloud.Firestore;

namespace Server.Models
{
    [FirestoreData]
    public class Group
    {
        [FirestoreDocumentId]
        public string GroupID { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string CreatorUID { get; set; }

        [FirestoreProperty]
        public List<string> Members { get; set; }
    }
}
