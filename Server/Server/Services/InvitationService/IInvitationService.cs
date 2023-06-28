using Server.Models;
using Server.Models.Responses;

namespace Server.Services.InvitationService
{
    public interface IInvitationService
    {
		Task<Response<List<Invitation>>> GetPendingInvitations(string userId);
		Task<BaseResponse> AcceptInvitation(string userId, string invitationId);
		Task<BaseResponse> RejectInvitation(string userId, string invitationId);
	}
}
