using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using Server.Models;
using Server.Models.Requests;
using Server.Models.Responses;
using Server.Services.GroupService;

namespace Server.Controllers
{
    [EnableCors]
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groupService;

        public GroupController(IGroupService friendshipService)
        {
            _groupService = friendshipService;
        }

        [HttpGet("GetGroups")]
        public async Task<ActionResult<Response<List<Group>>>> GetGroups()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _groupService.GetGroups(userId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("CreateGroup")]
        public async Task<ActionResult<Response<Group>>> CreateGroup(GroupRequestParameters request)
        {
            if (request.GroupName == null)
            {
                return BadRequest(BaseResponseDefaults.InvalidParametersResponse);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _groupService.CreateGroup(userId, request.GroupName);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("GetGroup")]
        public async Task<ActionResult<Response<Group>>> GetGroup(GroupRequestParameters request)
        {
            if (request.GroupID == null)
            {
                return BadRequest(BaseResponseDefaults.InvalidParametersResponse);
            }
  
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _groupService.GetGroup(userId, request.GroupID);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("GetGroupMembers")]
        public async Task<ActionResult<Response<List<User>>>> GetGroupMembers(GroupRequestParameters request)
        {
            if (request.GroupID == null)
            {
                return BadRequest(BaseResponseDefaults.InvalidParametersResponse);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _groupService.GetGroupMembers(userId, request.GroupID);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("GetGroupMessages")]
        public async Task<ActionResult<Response<List<Message>>>> GetGroupMessages(GroupRequestParameters request)
        {
            if (request.GroupID == null)
            {
                return BadRequest(BaseResponseDefaults.InvalidParametersResponse);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _groupService.GetGroupMessages(userId, request.GroupID);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("GetGroupMembersLastOnlineTime")]
        public async Task<ActionResult<Response<Dictionary<string, DateTime>>>> GetGroupMembersLastOnlineTime(GroupRequestParameters request)
        {
            if (request.GroupID == null)
            {
                return BadRequest(BaseResponseDefaults.InvalidParametersResponse);
            }
            
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _groupService.GetGroupMembersLastOnlineTime(userId, request.GroupID);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("LeaveGroup")]
        public async Task<ActionResult<Response<Group>>> LeaveGroup(GroupRequestParameters request)
        {
            if (request.GroupID == null)
            {
                return BadRequest(BaseResponseDefaults.InvalidParametersResponse);
            }
 
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _groupService.LeaveGroup(userId, request.GroupID);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("KickMember")]
        public async Task<ActionResult<Response<User>>> KickMember(GroupRequestParameters request)
        {
            if (request.GroupID == null || request.MemberID == null)
            {
                return BadRequest(BaseResponseDefaults.InvalidParametersResponse);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _groupService.KickMember(userId, request.GroupID, request.MemberID);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("SendMessage")]
        public async Task<ActionResult<Response<Message>>> SendMessage(GroupRequestParameters request)
        {
            if (request.GroupID == null || request.Message == null)
            {
                return BadRequest(BaseResponseDefaults.InvalidParametersResponse);
            }
    
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _groupService.SendMessage(userId, request.GroupID, request.Message);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("SendGroupInvitation")]
        public async Task<ActionResult<BaseResponse>> SendGroupInvitation(GroupRequestParameters request)
        {
            if (request.GroupID == null || request.MemberID == null)
            {
                return BadRequest(BaseResponseDefaults.InvalidParametersResponse);
            }
 
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _groupService.SendGroupInvitation(userId, request.GroupID, request.MemberID);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
