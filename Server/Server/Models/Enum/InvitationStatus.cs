using Google.Cloud.Firestore;

namespace Server.Models.Enum
{
    [FirestoreData(ConverterType = typeof(FirestoreEnumNameConverter<InvitationStatus>))]
    public enum InvitationStatus
    {
        Pending,
        Accepted,
        Rejected
    }
}
