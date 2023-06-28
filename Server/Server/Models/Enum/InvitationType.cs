using Google.Cloud.Firestore;

namespace Server.Models.Enum
{
    [FirestoreData(ConverterType = typeof(FirestoreEnumNameConverter<InvitationType>))]
    public enum InvitationType
    {
        Friendship,
        Group
    }
}
