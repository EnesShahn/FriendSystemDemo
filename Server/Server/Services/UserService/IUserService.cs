using Server.Models;
using Server.Models.Requests;
using Server.Models.Responses;

namespace Server.Services.UserService
{
    public interface IUserService
    {
        Task<Response<Token>> Register(RegisterUserRequest request);
        Task<Response<Token>> Login(LoginUserRequest request);
        Task<Response<User>> GetUser(string userId);
        Task<Response<DateTime>> GetUserLastOnlineTime(string userId);
    }
}
