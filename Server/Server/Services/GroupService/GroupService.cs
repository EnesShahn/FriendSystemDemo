using Google.Cloud.Firestore;

using Server.Models;
using Server.Models.Responses;

namespace Server.Services.GroupService
{
    public class GroupService : IGroupService
    {
        private FirestoreDb _firestoreDb;

        public GroupService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<Response<Group>> CreateGroup(string userId, string groupName)
        {
            var newGroupDocRef = _firestoreDb.Collection(CollectionConstants.GroupsCollections).Document();

            var newGroup = new Group
            {
                CreatorUID = userId,
                Name = groupName,
                Members = new List<string>(new List<string> { userId })
            };

            await newGroupDocRef.SetAsync(newGroup);

            var newGroupDocSnapshot = await newGroupDocRef.GetSnapshotAsync();

            return new Response<Group>
            {
                Success = true,
                Data = newGroupDocSnapshot.ConvertTo<Group>(),
            };
        }
        public async Task<Response<List<Group>>> GetGroups(string userId)
        {
            Filter isGroupExistFilter = Filter.EqualTo(FieldConstants.GroupCreatorID, userId);
            Filter isMemberOfGroupFilter = Filter.ArrayContains(FieldConstants.GroupMembers, userId);
            Filter isPartOfGroupFilter = Filter.Or(isGroupExistFilter, isMemberOfGroupFilter);

            var groupQuerySnapshot = await _firestoreDb.Collection(CollectionConstants.GroupsCollections)
                .Where(isPartOfGroupFilter).GetSnapshotAsync();

            List<Group> groups = new List<Group>();
            foreach (var groupDocSnapshot in groupQuerySnapshot.Documents)
            {
                groups.Add(groupDocSnapshot.ConvertTo<Group>());
            }

            return new Response<List<Group>>
            {
                Success = true,
                Data = groups
            };
        }
        public async Task<Response<Group>> GetGroup(string userId, string groupId)
        {
            var groupDocSnapshot = await _firestoreDb.Collection(CollectionConstants.GroupsCollections).Document(groupId).GetSnapshotAsync();
            if (!groupDocSnapshot.Exists)
            {
                return new Response<Group>
                {
                    Success = false,
                    Message = "Group doesn't exist"
                };
            }

            var group = groupDocSnapshot.ConvertTo<Group>();
            if (!group.Members.Contains(userId))
            {
                return new Response<Group>
                {
                    Success = false,
                    Message = "You are not part of the group"
                };
            }

            return new Response<Group>
            {
                Success = true,
                Data = group
            };
        }
        public async Task<Response<List<User>>> GetGroupMembers(string userId, string groupId)
        {
            var groupDocSnapshot = await _firestoreDb.Collection(CollectionConstants.GroupsCollections).Document(groupId).GetSnapshotAsync();
            if (!groupDocSnapshot.Exists)
            {
                return new Response<List<User>>
                {
                    Success = false,
                    Message = "Group doesn't exist"
                };
            }

            var group = groupDocSnapshot.ConvertTo<Group>();
            if (!group.Members.Contains(userId))
            {
                return new Response<List<User>>
                {
                    Success = false,
                    Message = "You are not part of the group"
                };
            }

            var membersQuerySnapshot = await _firestoreDb.Collection(CollectionConstants.UsersCollection)
                .WhereIn(FieldPath.DocumentId, group.Members).GetSnapshotAsync();

            List<User> members = new List<User>();
            foreach (var memberDocSnapshot in membersQuerySnapshot.Documents)
            {
                members.Add(memberDocSnapshot.ConvertTo<User>());
            }

            return new Response<List<User>>
            {
                Success = true,
                Data = members
            };
        }
        public async Task<Response<List<Message>>> GetGroupMessages(string userId, string groupId)
        {
            var groupDocSnapshot = await _firestoreDb.Collection(CollectionConstants.GroupsCollections).Document(groupId).GetSnapshotAsync();
            if (!groupDocSnapshot.Exists)
            {
                return new Response<List<Message>>
                {
                    Success = false,
                    Message = "Group doesn't exist"
                };
            }

            var group = groupDocSnapshot.ConvertTo<Group>();
            if (!group.Members.Contains(userId))
            {
                return new Response<List<Message>>
                {
                    Success = false,
                    Message = "You are not part of the group"
                };
            }

            var groupRef = groupDocSnapshot.Reference;

            var groupMessagesCollection = groupRef.Collection(CollectionConstants.MessagesCollection);

            List<Message> messages = new List<Message>();
            await foreach (var message in groupMessagesCollection.ListDocumentsAsync())
            {
                var messageSnapshot = await message.GetSnapshotAsync();
                messages.Add(messageSnapshot.ConvertTo<Message>());
            }

            return new Response<List<Message>>
            {
                Success = true,
                Data = messages
            };
        }
        public async Task<Response<Dictionary<string, DateTime>>> GetGroupMembersLastOnlineTime(string userId, string groupId)
        {
            var groupDocSnapshot = await _firestoreDb.Collection(CollectionConstants.GroupsCollections).Document(groupId).GetSnapshotAsync();
            if (!groupDocSnapshot.Exists)
            {
                return new Response<Dictionary<string, DateTime>>
                {
                    Success = false,
                    Message = "Group doesn't exist"
                };
            }

            var group = groupDocSnapshot.ConvertTo<Group>();
            if (!group.Members.Contains(userId))
            {
                return new Response<Dictionary<string, DateTime>>
                {
                    Success = false,
                    Message = "You are not part of the group"
                };
            }

            var membersQuerySnapshot = await _firestoreDb.Collection(CollectionConstants.UsersCollection)
                .WhereIn(FieldPath.DocumentId, group.Members).GetSnapshotAsync();

            Dictionary<string, DateTime> membersLastOnlineTimeMap = new Dictionary<string, DateTime>();
            foreach (var memberDocSnapshot in membersQuerySnapshot.Documents)
            {
                membersLastOnlineTimeMap.Add(memberDocSnapshot.Reference.Id, memberDocSnapshot.GetValue<DateTime>(FieldConstants.LastOnlineTime));
            }

            return new Response<Dictionary<string, DateTime>>
            {
                Success = true,
                Data = membersLastOnlineTimeMap
            };
        }

        public async Task<Response<Group>> LeaveGroup(string userId, string groupId)
        {
            var groupDocSnapshot = await _firestoreDb.Collection(CollectionConstants.GroupsCollections).Document(groupId).GetSnapshotAsync();
            if (!groupDocSnapshot.Exists)
            {
                return new Response<Group>
                {
                    Success = false,
                    Message = "Group doesn't exist"
                };
            }

            var group = groupDocSnapshot.ConvertTo<Group>();
            if (!group.Members.Contains(userId))
            {
                return new Response<Group>
                {
                    Success = false,
                    Message = "You are not part of the group"
                };
            }

            var groupDocRef = groupDocSnapshot.Reference;

            await _firestoreDb.RunTransactionAsync(async t =>
            {
                var groupSnapshot = await t.GetSnapshotAsync(groupDocRef);
                var group = groupSnapshot.ConvertTo<Group>();

                if (group.CreatorUID == userId)
                {
                    if (group.Members.Count == 1) // Delete the group
                    {
                        t.Delete(groupDocRef);
                    }
                    else if (group.Members.Count > 1) // Pass the leadership to another member
                    {
                        var nextMemberToBeLeaderID = group.Members.First(memberId => memberId != userId);
                        t.Update(groupSnapshot.Reference, FieldConstants.GroupCreatorID, nextMemberToBeLeaderID);
                        t.Update(groupSnapshot.Reference, FieldConstants.GroupMembers, FieldValue.ArrayRemove(userId));
                    }
                }
                else // Leave the group
                {
                    t.Update(groupSnapshot.Reference, FieldConstants.GroupMembers, FieldValue.ArrayRemove(userId));
                }
            });

            return new Response<Group>
            {
                Success = true,
                Message = "Left the group",
                Data = group
            };
        }
        public async Task<Response<User>> KickMember(string userId, string groupId, string memberId)
        {
            if (userId == memberId)
            {
                return new Response<User>
                {
                    Success = false,
                    Message = "You can't kick youself, you must leave the group"
                };
            }

            var groupDocSnapshot = await _firestoreDb.Collection(CollectionConstants.GroupsCollections).Document(groupId).GetSnapshotAsync();
            if (!groupDocSnapshot.Exists)
            {
                return new Response<User>
                {
                    Success = false,
                    Message = "Group doesn't exist"
                };
            }

            var group = groupDocSnapshot.ConvertTo<Group>();
            if (group.CreatorUID != userId)
            {
                return new Response<User>
                {
                    Success = false,
                    Message = "You don't have the premission to kick members"
                };
            }
            if (!group.Members.Contains(memberId))
            {
                return new Response<User>
                {
                    Success = false,
                    Message = "Member not part of the group"
                };
            }

            var groupDocRef = groupDocSnapshot.Reference;
            await groupDocRef.UpdateAsync(FieldConstants.GroupMembers, FieldValue.ArrayRemove(memberId));

            var memberDocSnapshot = await _firestoreDb.Collection(CollectionConstants.UsersCollection).Document(memberId).GetSnapshotAsync();

            var member = memberDocSnapshot.ConvertTo<User>();

            return new Response<User>
            {
                Success = true,
                Message = "Member kicked",
                Data = member
            };
        }
        public async Task<Response<Message>> SendMessage(string userId, string groupId, string message)
        {
            var groupDocSnapshot = await _firestoreDb.Collection(CollectionConstants.GroupsCollections).Document(groupId).GetSnapshotAsync();
            if (!groupDocSnapshot.Exists)
            {
                return new Response<Message>
                {
                    Success = false,
                    Message = "Group doesn't exist"
                };
            }

            var group = groupDocSnapshot.ConvertTo<Group>();
            if (!group.Members.Contains(userId))
            {
                return new Response<Message>
                {
                    Success = false,
                    Message = "You are not part of the group"
                };
            }

            var groupDocRef = groupDocSnapshot.Reference;
            var newMessageDocRef = groupDocRef.Collection(CollectionConstants.MessagesCollection).Document();
            MessageOut newMessage = new MessageOut
            {
                SenderID = userId,
                Content = message,
                CreateTime = FieldValue.ServerTimestamp
            };
            await newMessageDocRef.CreateAsync(newMessage);

            var newMessageSnapshot = await newMessageDocRef.GetSnapshotAsync();

            return new Response<Message>
            {
                Success = true,
                Data = newMessageSnapshot.ConvertTo<Message>()
            };
        }
        public async Task<BaseResponse> SendGroupInvitation(string userId, string groupId, string memberIdToInvite)
        {
            if (userId == memberIdToInvite)
            {
                return new BaseResponse
                {
                    Success = false,
                    Message = "You can't invite yourself, duh.."
                };
            }

            var groupDocSnapshot = await _firestoreDb.Collection(CollectionConstants.GroupsCollections).Document(groupId).GetSnapshotAsync();
            if (!groupDocSnapshot.Exists)
            {
                return new Response<List<Message>>
                {
                    Success = false,
                    Message = "Group doesn't exist"
                };
            }

            var group = groupDocSnapshot.ConvertTo<Group>();
            if (group.CreatorUID != userId)
            {
                return new Response<List<Message>>
                {
                    Success = false,
                    Message = "You don't have the premission to invite players"
                };
            }
            if (group.Members.Contains(memberIdToInvite))
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
                InvitationType = Models.Enum.InvitationType.Group,
                SenderID = groupId,
                ReceiverID = memberIdToInvite,
            };

            await newInvitationDocRef.CreateAsync(newInvitation);

            return new BaseResponse
            {
                Success = true,
                Message = "Invitation sent"
            };
        }

    }
}
