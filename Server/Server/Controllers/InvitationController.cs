using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using Server.Models;
using Server.Models.Requests;
using Server.Models.Responses;
using Server.Services.InvitationService;

namespace Server.Controllers
{
    [EnableCors]
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class InvitationController : ControllerBase
    {
        private readonly IInvitationService _invitationService;

        public InvitationController(IInvitationService invitationService)
        {
            _invitationService = invitationService;
        }


        [HttpGet("GetPendingInvitations")]
        public async Task<ActionResult<Response<List<Invitation>>>> GetPendingInvitations()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _invitationService.GetPendingInvitations(userId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("AcceptInvitation")]
        public async Task<ActionResult<BaseResponse>> AcceptInvitation(InvitationRequestParameters request)
        {
            if (request.InvitationID == null)
            {
                return BadRequest(BaseResponseDefaults.InvalidParametersResponse);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _invitationService.AcceptInvitation(userId, request.InvitationID);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("RejectInvitation")]
        public async Task<ActionResult<BaseResponse>> RejectInvitation(InvitationRequestParameters request)
        {
            if (request.InvitationID == null)
            {
                return BadRequest(BaseResponseDefaults.InvalidParametersResponse);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _invitationService.RejectInvitation(userId, request.InvitationID);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
