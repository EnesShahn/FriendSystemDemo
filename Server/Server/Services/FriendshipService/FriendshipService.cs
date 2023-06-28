using Google.Cloud.Firestore;

using Server.Models;
using Server.Models.Responses;
using Server.Services.UserService;

namespace Server.Services.FriendshipService
{
    public class FriendshipService : IFriendshipService
    {
        private FirestoreDb _firestoreDb;

        public FriendshipService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<Response<List<User>>> GetFriends(string userId)
        {
            var friendshipsQuery = await GetFriendshipsOf(userId);

            List<User> friends = new List<User>();
            foreach (var friendDocSnapshot in friendshipsQuery.Documents)
            {
                var friendship = friendDocSnapshot.ConvertTo<Friendship>();
                var friendId = userId == friendship.User1ID ? friendship.User2ID : friendship.User1ID;
                var friendSnapshot = await _firestoreDb.Collection(CollectionConstants.UsersCollection).Document(friendId).GetSnapshotAsync();

                if (!friendSnapshot.Exists) continue;

                var user = friendSnapshot.ConvertTo<User>();
                friends.Add(user);
            }

            return new Response<List<User>>
            {
                Success = true,
                Data = friends
            };
        }
        public async Task<Response<Dictionary<string, DateTime>>> GetFriendsLastOnlineTime(string userId)
        {
            var friendshipsQuery = await GetFriendshipsOf(userId);

            Dictionary<string, DateTime> friendsLastOnlineTimeMap = new Dictionary<string, DateTime>();
            foreach (var friendDocSnapshot in friendshipsQuery.Documents)
            {
                var friendship = friendDocSnapshot.ConvertTo<Friendship>();
                var friendId = userId == friendship.User1ID ? friendship.User2ID : friendship.User1ID;
                var friendSnapshot = await _firestoreDb.Collection(CollectionConstants.UsersCollection).Document(friendId).GetSnapshotAsync();

                if (!friendSnapshot.Exists) continue;

                var lastOnlineTime = friendSnapshot.GetValue<DateTime>(FieldConstants.LastOnlineTime);
                friendsLastOnlineTimeMap.Add(friendId, lastOnlineTime);
            }


            return new Response<Dictionary<string, DateTime>>
            {
                Success = true,
                Data = friendsLastOnlineTimeMap
            };
        }
        public async Task<Response<List<Message>>> GetFriendshipMessages(string userId, string friendId)
        {
            if (userId == friendId)
            {
                return new Response<List<Message>>
                {
                    Success = false,
                    Message = "Friend ID can't be the same as your ID"
                };
            }
            var friendshipDocRef = await GetFriendshipBetween(userId, friendId);
            if (friendshipDocRef == null)
            {
                return new Response<List<Message>>
                {
                    Success = false,
                    Message = "Friendship doesn't exist"
                };
            }

            var messagesQuerySnapshot = await friendshipDocRef.Collection(CollectionConstants.MessagesCollection).OrderBy(FieldConstants.CreateTime).GetSnapshotAsync();

            List<Message> messages = new List<Message>();
            foreach (var messageDocSnapshot in messagesQuerySnapshot.Documents)
            {
                messages.Add(messageDocSnapshot.ConvertTo<Message>());
            }

            return new Response<List<Message>>
            {
                Success = true,
                Data = messages
            };
        }
        public async Task<Response<User>> RemoveFriend(string userId, string friendId)
        {
            if (userId == friendId)
            {
                return new Response<User>
                {
                    Success = false,
                    Message = "Friend ID can't be the same as your ID"
                };
            }

            var friendshipDocRef = await GetFriendshipBetween(userId, friendId);
            if (friendshipDocRef == null)
            {
                return new Response<User>
                {
                    Success = false,
                    Message = "Friendship doesn't exist"
                };
            }

            await friendshipDocRef.DeleteAsync();

            var removedFriendDocSnapshot = await _firestoreDb.Collection(CollectionConstants.UsersCollection).Document(friendId).GetSnapshotAsync();
            var removedFriend = removedFriendDocSnapshot.ConvertTo<User>();
            return new Response<User>
            {
                Success = true,
                Message = "Friend removed",
                Data = removedFriend
            };
        }
        public async Task<Response<Message>> SendMessage(string userId, string friendId, string messageContent)
        {
            if (userId == friendId)
            {
                return new Response<Message>
                {
                    Success = false,
                    Message = "Friend ID can't be the same as your ID"
                };

            }
            var friendshipDocRef = await GetFriendshipBetween(userId, friendId);
            if (friendshipDocRef == null)
            {
                return new Response<Message>
                {
                    Success = false,
                    Message = "Not friends"
                };
            }

            var messagesCollection = friendshipDocRef.Collection(CollectionConstants.MessagesCollection);

            MessageOut newMessage = new MessageOut
            {
                SenderID = userId,
                Content = messageContent,
                CreateTime = FieldValue.ServerTimestamp
            };

            var newMessageDocRef = await messagesCollection.AddAsync(newMessage);
            var newMessageSnapshot = await newMessageDocRef.GetSnapshotAsync();

            return new Response<Message>
            {
                Success = true,
                Data = newMessageSnapshot.ConvertTo<Message>()
            };
        }
        public async Task<BaseResponse> SendFriendInvitation(string userId, string friendId)
        {
            if (userId == friendId)
            {
                return new BaseResponse
                {
                    Success = false,
                    Message = "Friend ID can't be the same as your ID"
                };
            }
            //Check if friend exists
            var friendQuerySnapshot = await _firestoreDb.Collection(CollectionConstants.UsersCollection).Document(friendId).GetSnapshotAsync();
            if (!friendQuerySnapshot.Exists)
            {
                return new BaseResponse
                {
                    Success = false,
                    Message = "Friend doesn't exist"
                };
            }

            var querySnapshot = await _firestoreDb.Collection(CollectionConstants.InvitationCollection)
                .WhereEqualTo(FieldConstants.SenderID, userId).WhereEqualTo(FieldConstants.ReceiverID, friendId).GetSnapshotAsync();

            if (querySnapshot.Count != 0)
            {
                return new BaseResponse
                {
                    Success = false,
                    Message = "Invite already sent"
                };
            }


            var newInvitationDocRef = _firestoreDb.Collection(CollectionConstants.InvitationCollection).Document();

            var newInvitation = new Invitation
            {
                InvitationType = Models.Enum.InvitationType.Friendship,
                SenderID = userId,
                ReceiverID = friendId,
            };


            await newInvitationDocRef.SetAsync(newInvitation);

            return new BaseResponse
            {
                Success = true,
                Message = "Invitation sent"
            };
        }

        private async Task<DocumentReference> GetFriendshipBetween(string userId, string friendId)
        {
            Filter userIdCheck1 = Filter.EqualTo(FieldConstants.User1ID, userId);
            Filter friendIdCheck1 = Filter.EqualTo(FieldConstants.User2ID, friendId);
            Filter firstAnd = Filter.And(userIdCheck1, friendIdCheck1);

            Filter userIdCheck2 = Filter.EqualTo(FieldConstants.User1ID, friendId);
            Filter friendIdCheck2 = Filter.EqualTo(FieldConstants.User2ID, userId);
            Filter secondAnd = Filter.And(userIdCheck2, friendIdCheck2);

            Filter orFilter = Filter.Or(firstAnd, secondAnd);

            var friendshipQuerySnapshot = await _firestoreDb.Collection(CollectionConstants.FriendshipCollection)
                .Where(orFilter).GetSnapshotAsync();

            if (friendshipQuerySnapshot.Count == 0) return null;

            return friendshipQuerySnapshot.Documents[0].Reference;

        }
        private async Task<QuerySnapshot> GetFriendshipsOf(string userId)
        {
            Filter userIdCheck1 = Filter.EqualTo(FieldConstants.User1ID, userId);
            Filter userIdCheck2 = Filter.EqualTo(FieldConstants.User2ID, userId);
            Filter orFilter = Filter.Or(userIdCheck1, userIdCheck2);

            var friendshipQuerySnapshot = await _firestoreDb.Collection(CollectionConstants.FriendshipCollection)
                .Where(orFilter).GetSnapshotAsync();
            return friendshipQuerySnapshot;

        }
    }
}
