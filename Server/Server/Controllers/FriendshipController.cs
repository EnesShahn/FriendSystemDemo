using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using Server.Models;
using Server.Models.Requests;
using Server.Models.Responses;
using Server.Services.FriendshipService;

namespace Server.Controllers
{
    [EnableCors]
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class FriendshipController : ControllerBase
    {
        private readonly IFriendshipService _friendshipService;

        public FriendshipController(IFriendshipService friendshipService)
        {
            _friendshipService = friendshipService;
        }


        [HttpGet("GetFriends")]
        public async Task<ActionResult<Response<List<User>>>> GetFriends()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _friendshipService.GetFriends(userId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("GetFriendsLastOnlineTime")]
        public async Task<ActionResult<Response<Dictionary<string, DateTime>>>> GetFriendsLastOnlineTime()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _friendshipService.GetFriendsLastOnlineTime(userId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("GetFriendshipMessages")]
        public async Task<ActionResult<Response<List<Message>>>> GetFriendshipMessages(FriendshipRequestParameters request)
        {
            if (request.FriendID == null)
            {
                return BadRequest(BaseResponseDefaults.InvalidParametersResponse);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _friendshipService.GetFriendshipMessages(userId, request.FriendID);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("RemoveFriend")]
        public async Task<ActionResult<Response<User>>> RemoveFriend(FriendshipRequestParameters request)
        {
            if (request.FriendID == null)
            {
                return BadRequest(BaseResponseDefaults.InvalidParametersResponse);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _friendshipService.RemoveFriend(userId, request.FriendID);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("SendMessage")]
        public async Task<ActionResult<Response<Message>>> SendMessage(FriendshipRequestParameters request)
        {
            if (request.FriendID == null || request.Message == null)
            {
                return BadRequest(BaseResponseDefaults.InvalidParametersResponse);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _friendshipService.SendMessage(userId, request.FriendID, request.Message);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("SendFriendInvitation")]
        public async Task<ActionResult<BaseResponse>> SendFriendInvitation(FriendshipRequestParameters request)
        {
            if (request.FriendID == null)
            {
                return BadRequest(BaseResponseDefaults.InvalidParametersResponse);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _friendshipService.SendFriendInvitation(userId, request.FriendID);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
