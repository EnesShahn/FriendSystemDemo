using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using Server.Models;
using Server.Models.Requests;
using Server.Models.Responses;
using Server.Services.UserService;

namespace Server.Controllers
{
    [EnableCors]
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<Response<Token>>> Register(RegisterUserRequest request)
        {
            if (request.Email == null || request.Password == null)
            {
                return BadRequest(BaseResponseDefaults.InvalidParametersResponse);
            }

            var response = await _userService.Register(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<Response<Token>>> Login(LoginUserRequest request)
        {
            if (request.Email == null || request.Password == null)
            {
                return BadRequest(BaseResponseDefaults.InvalidParametersResponse);
            }

            var response = await _userService.Login(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpGet("GetUser")]
        public async Task<ActionResult<Response<User>>> GetUser()
        {
            var userId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _userService.GetUser(userId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpGet("GetUserLastOnlineTime")]
        public async Task<ActionResult<Response<DateTime>>> GetUserLastOnlineTime()
        {
            var userId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var response = await _userService.GetUserLastOnlineTime(userId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
