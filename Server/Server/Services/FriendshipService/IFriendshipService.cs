using Server.Models;
using Server.Models.Responses;

namespace Server.Services.FriendshipService
{
    public interface IFriendshipService
    {
        Task<Response<List<User>>> GetFriends(string userId);
        Task<Response<Dictionary<string, DateTime>>> GetFriendsLastOnlineTime(string userId);
        Task<Response<List<Message>>> GetFriendshipMessages(string userId, string friendId);
        Task<Response<User>> RemoveFriend(string userId, string friendId);
        Task<Response<Message>> SendMessage(string userId, string friendId, string message);
        Task<BaseResponse> SendFriendInvitation(string userId, string friendId);
    }
}
