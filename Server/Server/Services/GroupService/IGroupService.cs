using Google.Cloud.Firestore;

using Server.Models;
using Server.Models.Responses;

namespace Server.Services.GroupService
{
    public interface IGroupService
    {
        Task<Response<Group>> CreateGroup(string userId, string groupName);
        Task<Response<List<Group>>> GetGroups(string userId);
        Task<Response<Group>> GetGroup(string userId, string groupId);
        Task<Response<List<User>>> GetGroupMembers(string userId, string groupId);
        Task<Response<List<Message>>> GetGroupMessages(string userId, string groupId);
        Task<Response<Dictionary<string, DateTime>>> GetGroupMembersLastOnlineTime(string userId, string groupId);
        Task<Response<Group>> LeaveGroup(string userId, string groupId);
        Task<Response<User>> KickMember(string userId, string groupId, string memberId);
        Task<Response<Message>> SendMessage(string userId, string groupId, string message);
        Task<BaseResponse> SendGroupInvitation(string userId, string groupId, string memberIdToInvite);
    }
}
