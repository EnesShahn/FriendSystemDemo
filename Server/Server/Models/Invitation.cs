using Google.Cloud.Firestore;
using Server.Models.Enum;

namespace Server.Models
{
    [FirestoreData]
    public class Invitation
    {
        [FirestoreDocumentId]
        public string InvitationID { get; set; }

        [FirestoreProperty]
        public InvitationType InvitationType { get; set; }

        [FirestoreProperty]
        public string SenderID { get; set; }

        [FirestoreProperty]
        public string ReceiverID { get; set; }
    }

}
