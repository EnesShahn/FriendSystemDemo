using System.Text.RegularExpressions;

using Google.Cloud.Firestore;

using Server.Models;
using Server.Models.Enum;
using Server.Models.Responses;

namespace Server.Services.InvitationService
{
    public class InvitationService : IInvitationService
    {
        private FirestoreDb _firestoreDb;

        public InvitationService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<Response<List<Invitation>>> GetPendingInvitations(string userId)
        {
            Filter isReceiverFilter = Filter.EqualTo(FieldConstants.ReceiverID, userId);
            var querySnapshot = await _firestoreDb.Collection(CollectionConstants.InvitationCollection).Where(isReceiverFilter).GetSnapshotAsync();

            List<Invitation> invitations = new List<Invitation>();
            foreach (var docSnapshot in querySnapshot.Documents)
            {
                var invitation = docSnapshot.ConvertTo<Invitation>();
                invitations.Add(invitation);
            }

            return new Response<List<Invitation>>
            {
                Success = true,
                Data = invitations
            };
        }
        public async Task<BaseResponse> AcceptInvitation(string userId, string invitationId)
        {
            var querySnapshot = await _firestoreDb.Collection(CollectionConstants.InvitationCollection).Document(invitationId).GetSnapshotAsync();

            if (!querySnapshot.Exists)
            {
                return new Response<Invitation>
                {
                    Success = false,
                    Message = "Invitation doesn't exist"
                };
            }

            var invitationDocRef = querySnapshot.Reference;
            var invitation = querySnapshot.ConvertTo<Invitation>();

            var oppositeInvitationQuerySnapshot = await _firestoreDb.Collection(CollectionConstants.InvitationCollection)
                .WhereEqualTo(FieldConstants.SenderID, userId).WhereEqualTo(FieldConstants.ReceiverID, invitation.SenderID).GetSnapshotAsync();


            if (invitation.InvitationType == InvitationType.Friendship)
            {
                var newFriendshipDocRef = _firestoreDb.Collection(CollectionConstants.FriendshipCollection).Document();
                Friendship newFriendship = new Friendship
                {
                    FriendshipID = newFriendshipDocRef.Id,
                    User1ID = invitation.SenderID,
                    User2ID = invitation.ReceiverID
                };

                await _firestoreDb.RunTransactionAsync(async (t) =>
                {
                    if (oppositeInvitationQuerySnapshot.Count != 0)
                    {
                        var oppositeInvitationDocRef = oppositeInvitationQuerySnapshot.Documents[0].Reference;
                        t.Delete(oppositeInvitationDocRef);
                    }
                    t.Delete(invitationDocRef);
                    t.Create(newFriendshipDocRef, newFriendship);
                });
            }
            else if (invitation.InvitationType == InvitationType.Group)
            {
                var groupDocSnapshot = await _firestoreDb.Collection(CollectionConstants.GroupsCollections).Document(invitation.SenderID).GetSnapshotAsync();
                if (!groupDocSnapshot.Exists)
                {
                    // Group got deleted
                    await invitationDocRef.DeleteAsync();
                    return new Response<Invitation>
                    {
                        Success = false,
                        Message = "Group is deleted"
                    };
                }
                var groupDocRef = groupDocSnapshot.Reference;

                await _firestoreDb.RunTransactionAsync(async (t) =>
                {
                    t.Delete(invitationDocRef);
                    t.Update(groupDocRef, FieldConstants.GroupMembers, FieldValue.ArrayUnion(userId));
                });
            }

            return new Response<Invitation>
            {
                Success = true,
                Message = "Invitation accepted"
            };
        }
        public async Task<BaseResponse> RejectInvitation(string userId, string invitationId)
        {
            var querySnapshot = await _firestoreDb.Collection(CollectionConstants.InvitationCollection).Document(invitationId).GetSnapshotAsync();
            if (!querySnapshot.Exists)
            {
                return new Response<Invitation>
                {
                    Success = false,
                    Message = "Invitation doesn't exist"
                };
            }

            var invitationDocRef = querySnapshot.Reference;
            await invitationDocRef.DeleteAsync();

            return new Response<Invitation>
            {
                Success = true,
                Message = "Invitation rejected"
            };
        }
    }
}
